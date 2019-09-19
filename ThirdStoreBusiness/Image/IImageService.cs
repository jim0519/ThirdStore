using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdStoreCommon.Models.Image;


namespace ThirdStoreBusiness.Image
{
    public interface IImageService
    {
        D_Image GetImageByID(int id);

        D_Image InsertImage(D_Image image);

        D_Image UpdateImage(D_Image image);

        void DeleteImage(D_Image image);

        D_Image SaveImage(Stream imgStream, string fileName);

        string GetImageURL(int imageID);
    }
}
