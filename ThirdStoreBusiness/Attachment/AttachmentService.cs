
using System;
using System.IO;
using System.Drawing;
using ThirdStoreCommon.Models.Attachment;
using ThirdStoreData;
using ThirdStoreCommon;
using System.Drawing.Drawing2D;
using ThirdStoreCommon.Infrastructure;
using System.Drawing.Imaging;

namespace ThirdStoreBusiness.Attachment
{

    public interface IAttachmentService
    {
        D_Attachment GetAttachmentByID(int id);

        D_Attachment InsertAttachment(D_Attachment attachment);

        D_Attachment UpdateAttachment(D_Attachment attachment);

        D_Attachment SaveAttachment(Stream fileStream, string fileName);

        string GetAttachmentURL(int attachmentID);

        void DeleteAttachment(D_Attachment attachment);
    }

    public class AttachmentService : IAttachmentService
    {
        private IRepository<D_Attachment> _attachmentRepository;
        private IWorkContext _workContext;

        public AttachmentService(IRepository<D_Attachment> attachmentRepository,
            IWorkContext workContext)
        {
            _attachmentRepository = attachmentRepository;
            _workContext = workContext;
        }



        public D_Attachment InsertAttachment(D_Attachment attachment)
        {
            if (attachment != null)
                _attachmentRepository.Insert(attachment);

            return attachment;
        }

        public D_Attachment UpdateAttachment(D_Attachment attachment)
        {
            if (attachment != null)
                _attachmentRepository.Update(attachment);

            return attachment;
        }

        public void DeleteAttachment(D_Attachment attachment)
        {
            if (attachment != null)
                _attachmentRepository.Delete(attachment);
        }

        public D_Attachment SaveAttachment(Stream fileStream,string fileName)
        {
            try
            {
                var currentUser = (_workContext.CurrentUser != null ? _workContext.CurrentUser.Name : Constants.SystemUser);

                using (var ms = new MemoryStream())
                {
                    if (System.Web.MimeMapping.GetMimeMapping(fileName).StartsWith("image/"))
                    {

                        var img = System.Drawing.Image.FromStream(fileStream);
                        if (img.Width > 1000 || img.Height > 1000)
                        {
                            img = ResizeAttachment(img, new Size(1000, 1000));
                        }
                        else
                        {
                            img = ResizeAttachment(img, new Size(img.Width, img.Height));
                        }

                        img.Save(ms, ImageFormat.Jpeg);

                    }
                    else
                    {
                        fileStream.CopyTo(ms);
                    }

                    var entityAttachment = new D_Attachment() { Name = fileName, LocalPath = string.Empty, CreateBy = currentUser, CreateTime = DateTime.Now, EditBy = currentUser, EditTime = DateTime.Now };
                    entityAttachment = this.InsertAttachment(entityAttachment);
                    string savefileName = string.Format("{0}{1}", entityAttachment.ID.ToString("0000000"), Path.GetExtension(fileName));
                    if (!Directory.Exists(ThirdStoreConfig.Instance.ThirdStoreAttachmentsPath))
                    {
                        Directory.CreateDirectory(ThirdStoreConfig.Instance.ThirdStoreAttachmentsPath);
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    using (FileStream fs = new FileStream(ThirdStoreConfig.Instance.ThirdStoreAttachmentsPath+"\\"+savefileName, FileMode.OpenOrCreate))
                    {
                        ms.CopyTo(fs);
                        fs.Flush();
                    }

                    return entityAttachment;
                }

                
              
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return default(D_Attachment);
            }
        }

        
        private System.Drawing.Image ResizeAttachment(System.Drawing.Image attachment,Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = attachment.Width;
                int originalHeight = attachment.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            System.Drawing.Image newAttachment = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newAttachment))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(attachment, 0, 0, newWidth, newHeight);
            }
            return newAttachment;
        }

        public D_Attachment GetAttachmentByID(int id)
        {
            return _attachmentRepository.GetById(id);
        }

        public string GetAttachmentURL(int attachmentID)
        {
            var url = $"{ThirdStoreConfig.Instance.ThirdStoreDomainURL}/ThirdStoreAttachmentsPath/{string.Format("{0}{1}", attachmentID.ToString("0000000"), Path.GetExtension(GetAttachmentByID(attachmentID).Name))}";
            return url;
        }

        public string GetLocalAttachmentPathByURL(string attachmentUrl)
        {
            var fileName = attachmentUrl.Substring(attachmentUrl.LastIndexOf("/") + 1);
            return ThirdStoreConfig.Instance.ThirdStoreAttachmentsPath + "\\" + fileName;
        }

        
    }
}
