
using System;
using System.IO;
using System.Drawing;
using ThirdStoreCommon.Models.Image;
using ThirdStoreData;
using ThirdStoreCommon;
using System.Drawing.Drawing2D;
using ThirdStoreCommon.Infrastructure;
using System.Drawing.Imaging;

namespace ThirdStoreBusiness.Image
{
    public class ImageService:IImageService
    {
        private IRepository<D_Image> _imageRepository;
        private IWorkContext _workContext;

        public ImageService(IRepository<D_Image> imageRepository,
            IWorkContext workContext)
        {
            _imageRepository = imageRepository;
            _workContext = workContext;
        }



        public D_Image InsertImage(D_Image image)
        {
            if (image != null)
                _imageRepository.Insert(image);

            return image;
        }

        public D_Image UpdateImage(D_Image image)
        {
            if (image != null)
                _imageRepository.Update(image);

            return image;
        }

        public void DeleteImage(D_Image image)
        {
            if (image != null)
                _imageRepository.Delete(image);
        }

        public D_Image SaveImage(Stream imgStream,string fileName)
        {
            try
            {
                var img = System.Drawing.Image.FromStream(imgStream);
                if (img.Width > 1000 || img.Height > 1000)
                {
                    img = ResizeImage(img,new Size(1000, 1000));
                }
                else
                {
                    img = ResizeImage(img,new Size(img.Width, img.Height));
                }

                var currentUser = (_workContext.CurrentUser != null ? _workContext.CurrentUser.Name : Constants.SystemUser);
                var entityImg = new D_Image() {  ImageName=fileName, ImageLocalPath=string.Empty, CreateBy= currentUser, CreateTime=DateTime.Now, EditBy= currentUser, EditTime= DateTime.Now };
                entityImg=this.InsertImage(entityImg);
                string savefileName = string.Format("{0}.{1}", entityImg.ID.ToString("0000000"), "jpg");
                if (!Directory.Exists(ThirdStoreConfig.Instance.ThirdStoreImagesPath))
                {
                    Directory.CreateDirectory(ThirdStoreConfig.Instance.ThirdStoreImagesPath);
                }
                img.Save(ThirdStoreConfig.Instance.ThirdStoreImagesPath+"\\"+ savefileName, ImageFormat.Jpeg);
                return entityImg;
                //Image img = Image.FromFile(imgFile);
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error(ex.Message);
                return default(D_Image);
            }
        }

        private System.Drawing.Image ResizeImage(System.Drawing.Image image,Size size, bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
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
            System.Drawing.Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public D_Image GetImageByID(int id)
        {
            return _imageRepository.GetById(id);
        }

        public string GetImageURL(int imageID)
        {
            var url = $"{ThirdStoreConfig.Instance.ThirdStoreDomainURL}/ThirdStoreImagesPath/{string.Format("{0}.{1}", imageID.ToString("0000000"), "jpg")}";
            return url;
        }
    }
}
