

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using network2.Models;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class PhotoController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _appContext;
        private PhotoService _photoService;
        public PhotoController(ApplicationContext context, IWebHostEnvironment iWebHostEnvironment)
        {
            _appContext = context;
            _photoService = new PhotoService(_appContext);
        }

        [HttpGet]
        public List<Comment> GetCommentsByPhotoId(int id)
        {
            PhotoModel thisPhoto = _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id);
            return _photoService.BuildCommentWithUser(_appContext.CommentModels.Where(cm => thisPhoto.Comments.Contains(cm.Id)).OrderByDescending(cm => cm.Time).ToList());
        }

        [HttpGet]
        public bool IsLikedByUser(int id)
        {
            UserModel user = _appContext.UserModels.FirstOrDefault(um => um.Nickname == User.Identity.Name);
            return _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id).LikeUsers.Contains(user.Id);
        }

        [HttpPost]
        public void LikeButtonResponse(int id)
        {
            UserModel user = _appContext.UserModels.FirstOrDefault(um => um.Nickname == User.Identity.Name);
            if (!_appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id).LikeUsers.Contains(user.Id))
            {
                _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id).LikeUsers
                    .Add(user.Id);
                _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id).Rating++;
            }
            else
            {
                _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id).LikeUsers
                    .Remove(user.Id);
                _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == id).Rating--;
            }

            _appContext.SaveChanges();
        }
        [HttpGet]
        public Comment SaveAndReturnComment(string text, int photoId)
        {
            DateTime now = DateTime.Now;
            CommentModel commentModel = new CommentModel
            {
                Text = text, 
                Time = now, 
                OwnerId = _appContext.UserModels.FirstOrDefault( um => um.Nickname == User.Identity.Name).Id, 
                PostId = photoId,
            };
            _appContext.CommentModels.Add(commentModel);
            _appContext.SaveChanges();
            _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == photoId).Comments
                .Add(_appContext.CommentModels
                    .FirstOrDefault(cm => cm.Text == text 
                                          && cm.OwnerId ==_appContext.UserModels.FirstOrDefault( um => um.Nickname == User.Identity.Name).Id &&
                                          cm.PostId == photoId && cm.Time == now).Id);
            _appContext.PhotoModels.FirstOrDefault(pm => pm.Id == photoId).CommentQuantity 
                = _appContext.PostModels.FirstOrDefault(pm => pm.Id == photoId).Comments.Count;

            _appContext.SaveChanges();
            Comment comment = new Comment()
            {
                Text = text, 
                Time = DateTime.Now, 
                OwnerId = _appContext.UserModels.FirstOrDefault( um => um.Nickname == User.Identity.Name).Id, 
                PostId = photoId,
                Owner = Mappers.BuildUser(_appContext.UserModels.FirstOrDefault(um => um.Id == commentModel.OwnerId))
            };
            return comment;
            }

    }
}