using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace manager.Helpers
{
    public class ImageSharp
    {
        public static byte[] ResizeImageWithImageSharp(Stream input)
        {
            // copy stream ไป memory stream ก่อน เพื่อความแน่ใจ
            using var memoryStream = new MemoryStream();
            input.CopyTo(memoryStream);
            memoryStream.Position = 0;

            using var image = Image.Load(memoryStream); // ตรงนี้จะไม่เจอปัญหาแล้ว
            image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

            using var outputStream = new MemoryStream();
            image.SaveAsPng(outputStream);
            return outputStream.ToArray();
        }

    }
}
