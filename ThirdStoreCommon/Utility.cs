using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.IO;
using Autofac;
using System.Web.Hosting;
using System.Web;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Linq.Expressions;
using System.Net;
using ThirdStoreCommon.Models;
using System.Xml;
using System.Diagnostics;
using static ThirdStoreCommon.Infrastructure.CacheFunc;
using ThirdStoreCommon.Infrastructure;
using System.Globalization;
using System.ComponentModel;
using Microsoft.Reporting.WebForms;
using System.Drawing.Printing;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Drawing.Imaging;
using System.Drawing;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace ThirdStoreCommon
{
    #region ThirdStoreWebClient
    public class ThirdStoreWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 30 * 60 * 1000;
            return w;
        }
    }
    #endregion

    #region Log Manager
    public class LogManager
    {
        private static ILog _instance;
        private LogManager()
        {

        }

        public static ILog Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = log4net.LogManager.GetLogger("CustomLogger");
                }
                return _instance;
            }
        }

    }
    #endregion

    #region Common Interface

    public interface IObjectParser
    {
        T ParseStringToObject<T>(string objString);
    }

    public interface ITypeFinder
    {
        IList<Assembly> GetAssemblies();

        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType<T, TAssemblyAttribute>(bool onlyConcreteClasses = true) where TAssemblyAttribute : Attribute;

        IEnumerable<Assembly> FindAssembliesWithAttribute<T>();

        IEnumerable<Assembly> FindAssembliesWithAttribute<T>(IEnumerable<Assembly> assemblies);

        IEnumerable<Assembly> FindAssembliesWithAttribute<T>(DirectoryInfo assemblyPath);
    }

    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder);

        int Order { get; }
    }

    #endregion

    #region Implementation class

    public class XMLObjectParser : IObjectParser
    {
        public T ParseStringToObject<T>(string objString)
        {
            try
            {
                if (string.IsNullOrEmpty(objString))
                {
                    return default(T);
                }
                var deserializer = new XmlSerializer(typeof(T));
                using (var strReader = new StringReader(objString))
                {
                    var returnObj = (T)deserializer.Deserialize(strReader);
                    return returnObj;
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }
            return default(T);
        }
    }

    //See details: https://stackoverflow.com/questions/5224697/deserializing-json-when-sometimes-array-and-sometimes-object
    public class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object retVal = new Object();
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }

    public class AppDomainTypeFinder : ITypeFinder
    {

        #region Fields

        private bool loadAppDomainAssemblies = true;
        private string assemblySkipLoadingPattern = "^System|^mscorlib|^Microsoft|^CppCodeProvider|^VJSharpCodeProvider|^WebDev|^Castle|^Iesi|^log4net|^NHibernate|^nunit|^TestDriven|^MbUnit|^Rhino|^QuickGraph|^TestFu|^Telerik|^ComponentArt|^MvcContrib|^AjaxControlToolkit|^Antlr3|^Remotion|^Recaptcha|^Zen.Barcode";
        private string assemblyRestrictToLoadingPattern = ".*";
        private IList<string> assemblyNames = new List<string>();

        /// <summary>
        /// Caches attributed assembly information so they don't have to be re-read
        /// </summary>
        private readonly List<AttributedAssembly> _attributedAssemblies = new List<AttributedAssembly>();
        /// <summary>
        /// Caches the assembly attributes that have been searched for
        /// </summary>
        private readonly List<Type> _assemblyAttributesSearched = new List<Type>();

        #endregion

        #region Constructor

        public AppDomainTypeFinder()
        {

        }

        #endregion

        #region Properties

        /// <summary>The app domain to look for types in.</summary>
        public virtual AppDomain App
        {
            get { return AppDomain.CurrentDomain; }
        }

        /// <summary>Gets or sets wether Nop should iterate assemblies in the app domain when loading Nop types. Loading patterns are applied when loading these assemblies.</summary>
        public bool LoadAppDomainAssemblies
        {
            get { return loadAppDomainAssemblies; }
            set { loadAppDomainAssemblies = value; }
        }

        /// <summary>Gets or sets assemblies loaded a startup in addition to those loaded in the AppDomain.</summary>
        public IList<string> AssemblyNames
        {
            get { return assemblyNames; }
            set { assemblyNames = value; }
        }

        /// <summary>Gets the pattern for dlls that we know don't need to be investigated.</summary>
        public string AssemblySkipLoadingPattern
        {
            get { return assemblySkipLoadingPattern; }
            set { assemblySkipLoadingPattern = value; }
        }

        /// <summary>Gets or sets the pattern for dll that will be investigated. For ease of use this defaults to match all but to increase performance you might want to configure a pattern that includes assemblies and your own.</summary>
        /// <remarks>If you change this so that Nop assemblies arn't investigated (e.g. by not including something like "^Nop|..." you may break core functionality.</remarks>
        public string AssemblyRestrictToLoadingPattern
        {
            get { return assemblyRestrictToLoadingPattern; }
            set { assemblyRestrictToLoadingPattern = value; }
        }

        #endregion

        #region ITypeFinder Members

        public virtual IList<Assembly> GetAssemblies()
        {
            var addedAssemblyNames = new List<string>();
            var assemblies = new List<Assembly>();

            if (LoadAppDomainAssemblies)
                AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
            AddConfiguredAssemblies(addedAssemblyNames, assemblies);

            return assemblies;
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    foreach (var t in a.GetTypes())
                    {
                        if (assignTypeFrom.IsAssignableFrom(t) || (assignTypeFrom.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                        {
                            if (!t.IsInterface)
                            {
                                if (onlyConcreteClasses)
                                {
                                    if (t.IsClass && !t.IsAbstract)
                                    {
                                        result.Add(t);
                                    }
                                }
                                else
                                {
                                    result.Add(t);
                                }
                            }
                        }
                    }

                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                Debug.WriteLine(fail.Message, fail);

                throw fail;
            }
            return result;
        }

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType<T, TAssemblyAttribute>(bool onlyConcreteClasses = true) where TAssemblyAttribute : Attribute
        {
            var found = FindAssembliesWithAttribute<TAssemblyAttribute>();
            return FindClassesOfType<T>(found, onlyConcreteClasses);
        }

        public IEnumerable<Assembly> FindAssembliesWithAttribute<T>()
        {
            return FindAssembliesWithAttribute<T>(GetAssemblies());
        }

        public IEnumerable<Assembly> FindAssembliesWithAttribute<T>(IEnumerable<Assembly> assemblies)
        {
            //check if we've already searched this assembly);)
            if (!_assemblyAttributesSearched.Contains(typeof(T)))
            {
                var foundAssemblies = (from assembly in assemblies
                                       let customAttributes = assembly.GetCustomAttributes(typeof(T), false)
                                       where customAttributes.Any()
                                       select assembly).ToList();
                //now update the cache
                _assemblyAttributesSearched.Add(typeof(T));
                foreach (var a in foundAssemblies)
                {
                    _attributedAssemblies.Add(new AttributedAssembly { Assembly = a, PluginAttributeType = typeof(T) });
                }
            }

            //We must do a ToList() here because it is required to be serializable when using other app domains.
            return _attributedAssemblies
                .Where(x => x.PluginAttributeType.Equals(typeof(T)))
                .Select(x => x.Assembly)
                .ToList();
        }

        public IEnumerable<Assembly> FindAssembliesWithAttribute<T>(DirectoryInfo assemblyPath)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Iterates all assemblies in the AppDomain and if it's name matches the configured patterns add it to our list.
        /// </summary>
        /// <param name="addedAssemblyNames"></param>
        /// <param name="assemblies"></param>
        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Matches(assembly.FullName))
                {
                    if (!addedAssemblyNames.Contains(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                        addedAssemblyNames.Add(assembly.FullName);
                    }
                }
            }
        }

        /// <summary>
        /// Adds specificly configured assemblies.
        /// </summary>
        /// <param name="addedAssemblyNames"></param>
        /// <param name="assemblies"></param>
        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach (string assemblyName in AssemblyNames)
            {
                Assembly assembly = Assembly.Load(assemblyName);
                if (!addedAssemblyNames.Contains(assembly.FullName))
                {
                    assemblies.Add(assembly);
                    addedAssemblyNames.Add(assembly.FullName);
                }
            }
        }

        /// <summary>
        /// Check if a dll is one of the shipped dlls that we know don't need to be investigated.
        /// </summary>
        /// <param name="assemblyFullName">
        /// The name of the assembly to check.
        /// </param>
        /// <returns>
        /// True if the assembly should be loaded into Nop.
        /// </returns>
        protected virtual bool Matches(string assemblyFullName)
        {
            return !Matches(assemblyFullName, AssemblySkipLoadingPattern)
                   && Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
        }

        /// <summary>
        /// Check if a dll is one of the shipped dlls that we know don't need to be investigated.
        /// </summary>
        /// <param name="assemblyFullName">
        /// The assembly name to match.
        /// </param>
        /// <param name="pattern">
        /// The regular expression pattern to match against the assembly name.
        /// </param>
        /// <returns>
        /// True if the pattern matches the assembly name.
        /// </returns>
        protected virtual bool Matches(string assemblyFullName, string pattern)
        {
            return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Makes sure matching assemblies in the supplied folder are loaded in the app domain.
        /// </summary>
        /// <param name="directoryPath">
        /// The physical path to a directory containing dlls to load in the app domain.
        /// </param>
        protected virtual void LoadMatchingAssemblies(string directoryPath)
        {
            var loadedAssemblyNames = new List<string>();
            foreach (Assembly a in this.GetAssemblies())
            {
                loadedAssemblyNames.Add(a.FullName);
            }

            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (string dllPath in Directory.GetFiles(directoryPath, "*.dll"))
            {
                try
                {
                    var an = AssemblyName.GetAssemblyName(dllPath);
                    if (Matches(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
                    {
                        App.Load(an);
                    }
                }
                catch (BadImageFormatException ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Does type implement generic?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="openGeneric"></param>
        /// <returns></returns>
        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Nested classes

        private class AttributedAssembly
        {
            internal Assembly Assembly { get; set; }
            internal Type PluginAttributeType { get; set; }
        }

        #endregion
    }

    public class AppAllDLLTypeFinder : AppDomainTypeFinder
    {
        private bool _binFolderAssembliesLoaded = false;
        #region Ctor

        public AppAllDLLTypeFinder()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a physical disk path of \Bin directory
        /// </summary>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string GetBaseDirectory()
        {
            if (HostingEnvironment.IsHosted)
            {
                //hosted
                return HttpRuntime.BinDirectory;
            }
            else
            {
                //not hosted. For example, run either in unit tests
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public override IList<Assembly> GetAssemblies()
        {
            if (!_binFolderAssembliesLoaded)
            {
                _binFolderAssembliesLoaded = true;
                string binPath = GetBaseDirectory();
                //binPath = _webHelper.MapPath("~/bin");
                LoadMatchingAssemblies(binPath);
            }

            return base.GetAssemblies();
        }

        #endregion
    }

    #endregion

    #region Common Extension Method

    public static class ContainerManagerExtensions
    {
        public static Autofac.Builder.IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PerLifeStyle<TLimit, TActivatorData, TRegistrationStyle>(this Autofac.Builder.IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, ComponentLifeStyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case ComponentLifeStyle.LifetimeScope:
                    return builder.InstancePerLifetimeScope();
                case ComponentLifeStyle.Transient:
                    return builder.InstancePerDependency();
                case ComponentLifeStyle.Singleton:
                    return builder.SingleInstance();
                default:
                    return builder.SingleInstance();
            }
        }
    }

    public static class CommonExtension
    {

        public static IEnumerable<T> ToEnumerable<T>(this T entity)
        {
            yield return entity;
        }

        public static bool Like(this string toSearch, string toFind)
        {
            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(toSearch);
        }

        public static Expression<Func<T>> ToExpression<T>(Func<T> call)
        {
            MethodCallExpression methodCall = call.Target == null
                ? Expression.Call(call.Method)
                : Expression.Call(Expression.Constant(call.Target), call.Method);

            return Expression.Lambda<Func<T>>(methodCall);
        }

        public static T FillOutNull<T>(this T obj) where T: class, new()
        {
            var t = typeof(T);
            foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
            {
                var originValue = p.GetValue(obj);
                if (originValue == null)
                {
                    switch (Type.GetTypeCode(p.PropertyType))
                    {
                        case TypeCode.String:
                            p.SetValue(obj, "", null);
                            break;
                        case TypeCode.Boolean:
                            p.SetValue(obj, false, null);
                            break;
                        case TypeCode.Decimal:
                            p.SetValue(obj, Convert.ToDecimal(0), null);
                            break;
                        case TypeCode.Single:
                            p.SetValue(obj, Convert.ToSingle(0), null);
                            break;
                        case TypeCode.Double:
                            p.SetValue(obj, Convert.ToDouble(0), null);
                            break;
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            p.SetValue(obj, 0, null);
                            break;
                        case TypeCode.Byte:
                        case TypeCode.Char:
                        case TypeCode.DateTime:
                            p.SetValue(obj, DateTime.Now, null);
                            break;
                        case TypeCode.DBNull:
                        case TypeCode.Empty:
                        case TypeCode.Object:
                        case TypeCode.SByte:
                        default:
                            break;
                    }
                }
            }

            return obj;
        }

        


        #region Name of

        public static string nameof<T, TProp>(this T obj, Expression<Func<T, TProp>> expression)
        {
            return nameof((LambdaExpression)expression);
        }

        private static string nameof(LambdaExpression expression)
        {
            MemberExpression memberExp = expression.Body as MemberExpression;
            if (memberExp != null)
                return memberExp.Member.Name;

            MethodCallExpression methodExp = expression.Body as MethodCallExpression;
            if (methodExp != null)
                return methodExp.Method.Name;

            return string.Empty;
        }

        #endregion

        #region Enum Conversion
        public static TEnum ToEnum<TEnum>(this string value)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        public static TEnum ToEnum<TEnum>(this int value)
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value); ;
        }

        public static int ToEnumValue<TEnum>(this string value)
        {
            return (int)Enum.Parse(typeof(TEnum), value);
        }

        public static string ToEnumName<TEnum>(this int value)
        {
            return Enum.GetName(typeof(TEnum), value);
        }

        public static int ToValue<TEnum>(this TEnum value)
        {
            return Convert.ToInt32(value);
        }

        public static string ToName<TEnum>(this TEnum value)
        {
            return value.ToString();
        }

        public static IDictionary<int,string> ToIDNameDicts<TEnum>(this TEnum value, int[] valuesToExclude = null)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("An Enumeration type is required.", "enumObj");
            }
            var items = from enumValue in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
                        where (valuesToExclude == null) || !valuesToExclude.Contains<int>(Convert.ToInt32(enumValue))
                        select new { Key = Convert.ToInt32(enumValue), Name = Enum.GetName(typeof(TEnum), enumValue) };
            var dict = new Dictionary<int, string>();
            foreach (var item in items)
                dict.Add(item.Key, item.Name);
            return dict;
        }
        #endregion

        public static string AsString(this XmlDocument xmlDoc)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    xmlDoc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }

        public static string GetSubstring(this string str,string startAt="",string stopAt="")
        {
            var startIndex = 0;
            if(!string.IsNullOrEmpty( startAt))
            {
                var idx = str.IndexOf(startAt);
                if (idx != -1)
                    startIndex = idx;
            }

            var stopIndex = str.Length-1;
            if (!string.IsNullOrEmpty(stopAt))
            {
                var idx = str.Substring(startIndex,str.Length-startIndex).IndexOf(stopAt);
                if (idx != -1)
                    stopIndex = idx+startIndex;
            }

            return str.Substring(startIndex, stopIndex - startIndex+1);
        }

        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: return string.Empty;
                case "": return string.Empty;
                default: return char.ToUpper(input.First()) + input.Substring(1).ToLower();
            }
        }

        public static bool IsNumeric(this string s)
        {
            //if (string.IsNullOrWhiteSpace(s))
            //    return false;
            float output;
            return float.TryParse(s, out output);
        }

        //public static

        public static IEnumerable<TSource> TakeWhileAggregate<TSource, TAccumulate>(
            this IEnumerable<TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func,
            Func<TAccumulate, TSource,bool> pushInPredicate,
            //Func<TAccumulate, TSource, bool> breakPredicate,
            Func<TAccumulate, TSource, bool> clearAllPredicate)
        {
            TAccumulate accumulator = seed;
            var retList = new List<TSource>();
            foreach (TSource item in source)
            {
                accumulator = func(accumulator, item);
                if (pushInPredicate(accumulator,item))
                {
                    //yield return item;
                    retList.Add(item);
                }
                else if(clearAllPredicate(accumulator, item))
                {
                    retList.Clear();
                }
                else
                {
                    retList.Add(item);
                    break;
                }
            }
            return retList;
        }
    }

#endregion

    #region Common Function

    public class CommonFunc
    {
        // regex \b(box|locker)(?:\b$|([\s|\-]+)?[0-9]+)|(p[\-\.\s]?o.?[\-\s]?|post office\s)b(\.|ox)?|parcel
        private static string[] POBoxLike = { "%P%BOX%", "%P%B%O%X%", "%CMB%", "%PBX%", "%POST%OFFICE%", "BOX%", "%GPO%", "%LPO%", "%L.P.O%", "%PA%CEL%LOCK%", "%PA%CEL%COLLECT%", "%PMB%", "%LOCK%BAG%", "%MAIL%CENTRE%", "%MAIL%CENTER%","%LOCKER%" };
        public static string GetOnlyFileNameByFullName(string fileFullName)
        {
            return fileFullName.Substring(fileFullName.LastIndexOf("\\") + 1, fileFullName.Length - (fileFullName.LastIndexOf("\\") + 1));
        }

        public static bool IsPOBox(string str)
        {
            return POBoxLike.Any(ps => str.Like(ps));
        }

        public static string ConvertXMLFromUTF8ToUTF16(string utf8XMLStr)
        {
            string content;
            // Create a memoryStream into which the data can be written and readed
            using (var stream = new MemoryStream())
            {
                // Create a XmlTextWriter to write the xml object source, we are going
                // to define the encoding in the constructor
                using (var writer = new XmlTextWriter(stream, Encoding.Unicode))
                {
                    // write the content into the stream
                    writer.WriteString(utf8XMLStr);

                    // Flush the stream
                    writer.Flush();

                    // Read the stream into a string
                    using (var reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        // Set the stream position to the begin
                        stream.Position = 0;

                        // Read the stream into a string
                        content = reader.ReadToEnd();
                    }
                }
            }
            return content;
        }

        public static string ConvertObjectToUTF16XMLString<T>(T obj)
        {
            return ConvertObjectToXMLString<T>(obj, Encoding.Unicode);
        }

        public static string ConvertObjectToXMLString<T>(T obj, Encoding encoding)
        {
            string content;
            // Create a memoryStream into which the data can be written and readed
            using (var stream = new MemoryStream())
            {

                // Create the xml serializer, the serializer needs to know the type
                // of the object that will be serialized
                var xmlSerializer = new XmlSerializer(typeof(T));

                // Create a XmlTextWriter to write the xml object source, we are going
                // to define the encoding in the constructor
                using (var writer = new XmlTextWriter(stream, encoding))
                {
                    // Save the state of the object into the stream
                    xmlSerializer.Serialize(writer, obj);

                    // Flush the stream
                    writer.Flush();

                    // Read the stream into a string
                    using (var reader = new StreamReader(stream, encoding))
                    {
                        // Set the stream position to the begin
                        stream.Position = 0;

                        // Read the stream into a string
                        content = reader.ReadToEnd();
                    }
                }
            }
            return content;
        }

        public static T RestfulXMLCall<T>(string restURL) where T : XMLObject
        {
            using (var webClient = new ThirdStoreWebClient())
            {
                var xmlParser = new XMLObjectParser();
                var resultString = webClient.DownloadString(restURL);

                var returnObj = xmlParser.ParseStringToObject<T>(resultString);

                if (returnObj != null)
                    returnObj.xmlString = resultString;

                return returnObj;
            }
        }

        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new List<T>(array);
            return list;
        }

        public static string ToCSVFileName(string prefix)
        {
            var sb = new StringBuilder();
            sb.Append(prefix);
            sb.Append("_");
            sb.Append(DateTime.Now.ToString("yyyyMMddHHmmss"));
            sb.Append(".csv");
            return sb.ToString();
        }

        public static string GetItemNumberOrTransactionID(string strID,eBayIDType type)
        {
            if (type == eBayIDType.ItemID)
                return strID.Substring(0, strID.IndexOf("-"));
            else
                return strID.Substring(strID.IndexOf("-") + 1, strID.Length - strID.IndexOf("-") - 1);
        }

        public static string RemoveMultiValuedAttribut(string fieldValue, string removeStr)
        {
            return RemoveMultiValuedAttribut(fieldValue, removeStr, new char[] { ';' });
        }
        public static string RemoveMultiValuedAttribut(string fieldValue, string removeStr, char[] delimiter)
        {
            if (string.IsNullOrEmpty(fieldValue))
                return fieldValue;

            var lstFieldValue = fieldValue.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstFieldValue.Contains(removeStr))
                lstFieldValue.Remove(removeStr);
            return string.Join(new string(delimiter), lstFieldValue);
        }


        public static string AddMultiValuedAttribut(string fieldValue, string addStr)
        {
            return AddMultiValuedAttribut(fieldValue, addStr, new char[] { ';' });
        }
        public static string AddMultiValuedAttribut(string fieldValue, string addStr, char[] delimiter)
        {
            var lstFieldValue = fieldValue.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (string.IsNullOrEmpty(addStr))
                return fieldValue;

            if (lstFieldValue.Contains(addStr))
                return fieldValue;
            lstFieldValue.Add(addStr);
            return string.Join(new string(delimiter), lstFieldValue);
        }

        public static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }


        #region Func<>

        #endregion


        public static Stream GenerateStreamFrThirdStoretring(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        
        public static bool DoesImageExistRemotely(string uriToImage)
        {
            if (string.IsNullOrEmpty(uriToImage))
                return false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriToImage);

            request.Method = "HEAD";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (WebException) { return false; }
            catch
            {
                return false;
            }
        }

        public static T To<T>(object value)
        {
            //return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            return (T)To(value, typeof(T));
        }

        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }

        public static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value != null)
            {
                var sourceType = value.GetType();

                var destinationConverter = TypeDescriptor.GetConverter(destinationType);
                if (destinationConverter != null && destinationConverter.CanConvertFrom(value.GetType()))
                    return destinationConverter.ConvertFrom(null, culture, value);

                var sourceConverter = TypeDescriptor.GetConverter(sourceType);
                if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                    return sourceConverter.ConvertTo(null, culture, value, destinationType);

                if (destinationType.IsEnum && value is int)
                    return Enum.ToObject(destinationType, (int)value);

                if (!destinationType.IsInstanceOfType(value))
                    return Convert.ChangeType(value, destinationType, culture);
            }
            return value;
        }

        public static string StripHTML(string source)
        {
            try
            {
                string result;

                // Remove HTML Development formatting
                // Replace line breaks with space
                // because browsers inserts space
                result = source.Replace("\r", " ");
                // Replace line breaks with space
                // because browsers inserts space
                result = result.Replace("\n", " ");
                // Remove step-formatting
                result = result.Replace("\t", string.Empty);
                // Remove repeating spaces because browsers ignore them
                result = System.Text.RegularExpressions.Regex.Replace(result,
                                                                      @"( )+", " ");

                // Remove the header (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*head([^>])*>", "<head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*head( )*>)", "</head>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<head>).*(</head>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all scripts (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*script([^>])*>", "<script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*script( )*>)", "</script>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                //result = System.Text.RegularExpressions.Regex.Replace(result,
                //         @"(<script>)([^(<script>\.</script>)])*(</script>)",
                //         string.Empty,
                //         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<script>).*(</script>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // remove all styles (prepare first by clearing attributes)
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*style([^>])*>", "<style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"(<( )*(/)( )*style( )*>)", "</style>",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(<style>).*(</style>)", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert tabs in spaces of <td> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*td([^>])*>", "\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line breaks in places of <BR> and <LI> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*br( )*/*>", "\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*li( )*>", "\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // insert line paragraphs (double line breaks) in place
                // if <P>, <DIV> and <TR> tags
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*div([^>])*>", "\r\n\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*tr([^>])*>", "\r\n\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<( )*p([^>])*>", "\r\n\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // Remove remaining tags like <a>, links, images,
                // comments etc - anything that's enclosed inside < >
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"<[^>]*>", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // replace special characters:
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @" ", " ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&bull;", " * ",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lsaquo;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&rsaquo;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&trade;", "(tm)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&frasl;", "/",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&lt;", "<",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&gt;", ">",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&copy;", "(c)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&reg;", "(r)",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove all others. More can be added, see
                // http://hotwired.lycos.com/webmonkey/reference/special_characters/
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         @"&(.{2,6});", string.Empty,
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // for testing
                //System.Text.RegularExpressions.Regex.Replace(result,
                //       this.txtRegex.Text,string.Empty,
                //       System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                // make line breaking consistent
                //result = result.Replace("\n", "\r\n");

                // Remove extra line breaks and tabs:
                // replace over 2 breaks with 2 and over 4 tabs with 4.
                // Prepare first to remove any whitespaces in between
                // the escaped characters and remove redundant tabs in between line breaks
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r\n)( )+(\r\n)", "\r\n\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\t)", "\t\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\t)( )+(\r\n)", "\t\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r\n)( )+(\t)", "\r\n\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove redundant tabs
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r\n)(\t)+(\r\n)", "\r\n\r\n",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Remove multiple tabs following a line break with just one tab
                result = System.Text.RegularExpressions.Regex.Replace(result,
                         "(\r\n)(\t)+", "\r\n\t",
                         System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                // Initial replacement target string for line breaks
                string breaks = "\r\n\r\n\r\n";
                // Initial replacement target string for tabs
                string tabs = "\t\t\t\t\t";
                for (int index = 0; index < result.Length; index++)
                {
                    result = result.Replace(breaks, "\r\n\r\n");
                    result = result.Replace(tabs, "\t\t\t\t");
                    breaks = breaks + "\r\n";
                    tabs = tabs + "\t";
                }

                // That's it.
                return result;
            }
            catch
            {
                return string.Empty;
            }
        }

    }

    #endregion

    #region Report Printing

    #region Interface

    public interface IReportPrinting
    {
        PrintResult PrintReport();

        //void SetPrinter(string printerName);
    }

    public interface ILocalReportPrinting : IReportPrinting
    {
        //void SetReportPath(string reportPath);

        //void SetReportParameter(IDictionary<string, IEnumerable<string>> reportParams);

        //void SetReportDatasource(IDictionary<string, IEnumerable> datasources);

        void SetReportPrintingParameters(LocalReportPrintingParameter reportPrintingParameter);
    }

    public interface IServerReportPrinting : IReportPrinting
    {
        //void SetReportPath(string reportPath);

        //void SetReportParameter(IDictionary<string, IEnumerable<string>> reportParams);

        //void SetCredentials(ICredentials credentials);

        void SetReportPrintingParameters(ServerReportPrintingParameter reportPrintingParameter);
    }

    #endregion

    #region Implementation Class and Model Class

    public class LocalReportPrinting : ILocalReportPrinting
    {

        //private IEnumerable _datasource;
        //private string _reportPath;
        private string _printerName;
        //private IDictionary<string, IEnumerable<string>> _reportParams;
        private LocalReport _localReport;


        public LocalReportPrinting()
        {
            _localReport = new LocalReport();
        }

        public PrintResult PrintReport()
        {
            var response = new PrintResult();
            try
            {
                using (var printDoc = new ReportPrintDocument(_localReport))
                {
                    printDoc.PrinterSettings.PrinterName = _printerName;

                    printDoc.Print();
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                response.Success = false;
                response.Message = _printerName + " " + ex.ToString();
            }

            return response;
        }

        public void SetReportPrintingParameters(LocalReportPrintingParameter reportPrintingParameter)
        {
            //printer name
            _printerName = reportPrintingParameter.PrinterName;

            //report path
            _localReport.ReportPath = reportPrintingParameter.ReportPath;

            //set permission
            PermissionSet permissions = new PermissionSet(PermissionState.None);
            permissions.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            _localReport.SetBasePermissionsForSandboxAppDomain(permissions);

            //parameter
            var reportParaCollection = new ReportParameterCollection();
            foreach (var param in reportPrintingParameter.ReportParameter)
            {
                reportParaCollection.Add(new ReportParameter(param.Key, param.Value.ToArray()));
            }
            _localReport.SetParameters(reportParaCollection);

            //datasource
            _localReport.DataSources.Clear();
            foreach (var ds in reportPrintingParameter.Datasources)
            {
                _localReport.DataSources.Add(new ReportDataSource(ds.Key, ds.Value));
            }

            

        }
    }

    public abstract class ReportPrintingParameter
    {
        public abstract string ReportPath { get; }

        private IDictionary<string, IEnumerable<string>> _reportParameter = new Dictionary<string, IEnumerable<string>>();
        public virtual IDictionary<string, IEnumerable<string>> ReportParameter
        {
            get
            {
                return _reportParameter;
            }
        }

        private string _printerName = string.Empty;
        public virtual string PrinterName
        {
            get
            {
                return _printerName;
            }
            set
            {
                _printerName = value;
            }
        }
    }

    public abstract class LocalReportPrintingParameter : ReportPrintingParameter
    {
        private IDictionary<string, IEnumerable> _datasource = new Dictionary<string, IEnumerable>();
        public virtual IDictionary<string, IEnumerable> Datasources
        {
            get
            {
                return _datasource;
            }
        }
    }

    public abstract class ServerReportPrintingParameter : ReportPrintingParameter
    {
        public abstract ICredentials credentials { get; set; }
    }


    public class PrintResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// The ReportPrintDocument will print all of the pages of a ServerReport or LocalReport.
    /// The pages are rendered when the print document is constructed.  Once constructed,
    /// call Print() on this class to begin printing.
    /// </summary>
    public class ReportPrintDocument : PrintDocument
    {
        private PageSettings m_pageSettings;
        private int m_currentPage;
        private List<Stream> m_pages = new List<Stream>();
        //private List<Metafile> m_metaFiles = new List<Metafile>();

        public ReportPrintDocument(ServerReport serverReport)
            : this((Report)serverReport)
        {
            RenderAllServerReportPages(serverReport);
        }

        public ReportPrintDocument(LocalReport localReport)
            : this((Report)localReport)
        {
            RenderAllLocalReportPages(localReport);
        }

        private ReportPrintDocument(Report report)
        {
            // Set the page settings to the default defined in the report
            ReportPageSettings reportPageSettings = report.GetDefaultPageSettings();

            // The page settings object will use the default printer unless
            // PageSettings.PrinterSettings is changed.  This assumes there
            // is a default printer.
            m_pageSettings = new PageSettings();
            m_pageSettings.PaperSize = reportPageSettings.PaperSize;
            m_pageSettings.Margins = reportPageSettings.Margins;
            m_pageSettings.Landscape = reportPageSettings.IsLandscape;
            //m_pageSettings.Margins = new  Margins(75, 75, 75, 75);

            //this.PrintController = new PreviewPrintController();

        }

        //public PageSettings CustomPageSetting
        //{
        //    get { return m_pageSettings; }
        //}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (Stream s in m_pages)
                {
                    s.Dispose();
                }

                m_pages.Clear();

                //foreach (var p in m_metaFiles)
                //{
                //    p.Dispose();
                //}
                //m_metaFiles.Clear();
            }
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);

            m_currentPage = 0;
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            base.OnPrintPage(e);

            Stream pageToPrint = m_pages[m_currentPage];
            pageToPrint.Position = 0;

            // Load each page into a Metafile to draw it.
            using (Metafile pageMetaFile = new Metafile(pageToPrint))
            {
                Rectangle adjustedRect = new Rectangle(
                        //e.PageBounds.Left - (int)e.PageSettings.HardMarginX,
                        //e.PageBounds.Top - (int)e.PageSettings.HardMarginY,
                        e.PageBounds.Left,
                        e.PageBounds.Top,
                        e.PageBounds.Width,
                        e.PageBounds.Height
                        );

                // Draw a white background for the report
                e.Graphics.FillRectangle(Brushes.White, adjustedRect);


                // Draw the report content
                e.Graphics.DrawImage(pageMetaFile, adjustedRect);

                e.Graphics.Flush();

                // Prepare for next page.  Make sure we haven't hit the end.
                m_currentPage++;
                e.HasMorePages = m_currentPage < m_pages.Count;

            }

            //using (Metafile pageMetaFile = new Metafile(pageToPrint))
            //{
            //Metafile pageMetaFile = new Metafile(pageToPrint);
            //m_metaFiles.Add(pageMetaFile);
            //m_currentPage++;
            //e.HasMorePages = m_currentPage < m_pages.Count;

            //if (!e.HasMorePages)
            //{ 
            //    Rectangle adjustedRect = new Rectangle(
            //    //e.PageBounds.Left - (int)e.PageSettings.HardMarginX,
            //    //e.PageBounds.Top - (int)e.PageSettings.HardMarginY,
            //        e.PageBounds.Left,
            //        e.PageBounds.Top,
            //        e.PageBounds.Width,
            //        e.PageBounds.Height
            //        );
            //    foreach (var metaPage in m_metaFiles)
            //    {
            //        e.Graphics.FillRectangle(Brushes.White, adjustedRect);

            //        e.Graphics.DrawImage(metaPage, adjustedRect);
            //    }
            //}
            //}
        }

        protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
        {
            e.PageSettings = (PageSettings)m_pageSettings.Clone();
            //e.PageSettings = e.PageSettings;
        }

        private void RenderAllServerReportPages(ServerReport serverReport)
        {
            string deviceInfo = CreateEMFDeviceInfo();

            // Generating Image renderer pages one at a time can be expensive.  In order
            // to generate page 2, the server would need to recalculate page 1 and throw it
            // away.  Using PersistStreams causes the server to generate all the pages in
            // the background but return as soon as page 1 is complete.
            NameValueCollection firstPageParameters = new NameValueCollection();
            firstPageParameters.Add("rs:PersistStreams", "True");
            //firstPageParameters.Add("rs:PersistStreams", "False");

            // GetNextStream returns the next page in the sequence from the background process
            // started by PersistStreams.
            NameValueCollection nonFirstPageParameters = new NameValueCollection();
            nonFirstPageParameters.Add("rs:GetNextStream", "True");

            string mimeType;
            string fileExtension;
            Stream pageStream = serverReport.Render("IMAGE", deviceInfo, firstPageParameters, out mimeType, out fileExtension);

            // The server returns an empty stream when moving beyond the last page.
            while (pageStream.Length > 0)
            {
                m_pages.Add(pageStream);

                pageStream = serverReport.Render("IMAGE", deviceInfo, nonFirstPageParameters, out mimeType, out fileExtension);
            }
        }

        private void RenderAllLocalReportPages(LocalReport localReport)
        {
            string deviceInfo = CreateEMFDeviceInfo();

            Warning[] warnings;
            localReport.Render("IMAGE", deviceInfo, PageCountMode.Actual, LocalReportCreateStreamCallback, out warnings);
        }

        private Stream LocalReportCreateStreamCallback(
            string name,
            string extension,
            Encoding encoding,
            string mimeType,
            bool willSeek)
        {
            MemoryStream stream = new MemoryStream();
            m_pages.Add(stream);

            return stream;
        }

        private string CreateEMFDeviceInfo()
        {
            PaperSize paperSize = m_pageSettings.PaperSize;
            Margins margins = m_pageSettings.Margins;

            // The device info string defines the page range to print as well as the size of the page.
            // A start and end page of 0 means generate all pages.
            return string.Format(
                CultureInfo.InvariantCulture,
                "<DeviceInfo><OutputFormat>emf</OutputFormat><StartPage>0</StartPage><EndPage>0</EndPage><MarginTop>{0}</MarginTop><MarginLeft>{1}</MarginLeft><MarginRight>{2}</MarginRight><MarginBottom>{3}</MarginBottom><PageHeight>{4}</PageHeight><PageWidth>{5}</PageWidth><MediaType>1</MediaType></DeviceInfo>",
                ToInches(margins.Top),
                ToInches(margins.Left),
                ToInches(margins.Right),
                ToInches(margins.Bottom),
                //ToInches(paperSize.Height),
                //ToInches(paperSize.Width)
                ToInches(m_pageSettings.Landscape ? paperSize.Width : paperSize.Height),
                ToInches(m_pageSettings.Landscape ? paperSize.Height : paperSize.Width)
                );
        }

        private static string ToInches(int hundrethsOfInch)
        {
            double inches = hundrethsOfInch / 100.0;
            return inches.ToString(CultureInfo.InvariantCulture) + "in";
        }
    }


    #endregion

    

    #endregion

}
