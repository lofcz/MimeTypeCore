namespace MimeTypeCore.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CollisionTs()
    {
        using FileStream streamVideo = File.Open("files/video.ts", FileMode.Open);
        using FileStream streamTypescript = File.Open("files/typescript.ts", FileMode.Open);
        
        MimeTypeMap.TryGetMimeType(streamVideo, out string mimeTypeVideo);
        MimeTypeMap.TryGetMimeType(streamTypescript, out string mimeTypeTypescript);
        
        Assert.That(mimeTypeVideo, Is.EqualTo("video/mpeg"));
        Assert.That(mimeTypeTypescript, Is.EqualTo("text/typescript"));
    }
}