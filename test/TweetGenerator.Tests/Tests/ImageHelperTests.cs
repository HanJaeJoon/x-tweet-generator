﻿using TweetGenerator.utils;

namespace TweetGenerator.Tests.Tests;

public class ImageHelperTests
{
    [Fact]
    public void CompressImage_ReduceSize_Successfully()
    {
        var testImagePath = "Assets/sample.png";
        var fileContent = File.ReadAllBytes(testImagePath);

        var compressedImage = ImageHelper.CompressImage(fileContent);

        Assert.NotEmpty(compressedImage);
        Assert.True(fileContent.Length > compressedImage.Length);
    }
}