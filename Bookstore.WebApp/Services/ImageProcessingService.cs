﻿using Bookstore.WebApp.Entities;
using ImageResizer;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Web;

namespace Bookstore.WebApp.Services
{
    public class ImageProcessingService
    {
        private HttpServerUtilityBase server;

        public ImageProcessingService(HttpServerUtilityBase server)
        {
            this.server = server;
        }

        public void ProcessImage(Stream imageStream, BookListing listing)
        {
            var thumbnailResizeSettings = new ResizeSettings();
            thumbnailResizeSettings.Format = "jpg";
            thumbnailResizeSettings.MaxHeight = 500;
            thumbnailResizeSettings.MaxWidth = 300;
            thumbnailResizeSettings.Quality = 80;

            var newImageSettings = new ResizeSettings();
            newImageSettings.Format = "jpg";
            newImageSettings.MaxHeight = 800;
            newImageSettings.MaxWidth = 1200;
            newImageSettings.Quality = 80;

            var thumbnailBytes = this.ResizeImage(imageStream, thumbnailResizeSettings);
            var newImageBytes = this.ResizeImage(imageStream, newImageSettings);
            var imageName = Guid.NewGuid().ToString();
            var imagePath = this.GetImagesPath() + imageName + ".jpg";
            var thumbnailPath = this.GetImagesPath() + imageName + "_thumb.jpg";

            Directory.CreateDirectory(this.GetImagesPath());
            File.WriteAllBytes(imagePath, newImageBytes);
            File.WriteAllBytes(thumbnailPath, thumbnailBytes);

            listing.SetImage(imageName);
        }

        private string GetImagesPath()
        {
            var dataPath = ConfigurationManager.AppSettings["DataFolderRelativePath"];
            var imagesPath = "~/" + dataPath + "images/";
            return this.server.MapPath(imagesPath);
        }

        private byte[] ResizeImage(Stream imageStream, ResizeSettings settings)
        {
            imageStream.Position = 0;

            var resizedImageStream = new MemoryStream();
            ImageBuilder.Current.Build(imageStream, resizedImageStream, settings, false);

            return resizedImageStream.ToArray();
        }
    }
}