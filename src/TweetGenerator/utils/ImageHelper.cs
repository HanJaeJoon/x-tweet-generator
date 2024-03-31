using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace TweetGenerator.utils;

public static class ImageHelper
{
    public static byte[] CompressImage(byte[] originalImage, int width = 1024, int height = 1024)
    {
        using var image = Image.Load(originalImage);

        image.Mutate(x => x.Resize(width, height));

        var encoder = new JpegEncoder()
        {
            Quality = 50, // 1 ~ 100 (default: 75)
        };

        using var memoryStream = new MemoryStream();
        image.SaveAsJpeg(memoryStream, encoder);
        return memoryStream.ToArray();
    }
}
