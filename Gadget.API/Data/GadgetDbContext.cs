//======================================
// <copyright file="GadgetDbContext.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================

using Gadget.API.APIModel;
using Gadget.API.Helper;
using Gadget.API.Helper.Log_API_Count;
using Gadget.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Gadget.API.Data
{
    public class GadgetDbContext : DbContext
    {
        private IHttpContextAccessor _httpContext;
        public GadgetDbContext(DbContextOptions<GadgetDbContext> options)
           : base(options)
        {
        }
        public GadgetDbContext(DbContextOptions<GadgetDbContext> options,
            IHttpContextAccessor httpContext)
            : base(options)
        {
            this._httpContext = httpContext;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string ConnectionString = null;
            if (this._httpContext != null)
            {
                HttpContext httpContext = this._httpContext.HttpContext;
                if (httpContext != null)
                {
                    string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                    if (!string.IsNullOrEmpty(encryptedConnectionString))
                    {
                        ConnectionString = Security.Decrypt(encryptedConnectionString);
                        optionsBuilder.UseSqlServer(ConnectionString, opt => opt.UseRowNumberForPaging())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                        base.OnConfiguring(optionsBuilder);
                    }
                }
            }
        }

        public DbSet<ProductCategory> ProductCategorys { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Advantage> Advantages { get; set; }
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ProductAccessibility> ProductAccessibility { get; set; }
        //public DbSet<EmployeeSuggestionLike> EmployeeSuggestionLike { get; set; }

        public DbSet<WorkDiary> WorkDiary { get; set; }
        public DbSet<Banner> Banner { get; set; }
        public DbSet<UseCase> UseCase { get; set; }
        //public DbSet<EmployeeAwards> EmployeeAwards { get; set; }
        public DbSet<DigitalRole> DigitalRole { get; set; }
        //public DbSet<AwardList> AwardList { get; set; }
        public DbSet<DigitalAdoptionReview> DigitalAdoptionReview { get; set; }

        //public DbSet<SuggestionCategory> SuggestionCategory { get; set; }
        //public DbSet<EmployeeSuggestions> EmployeeSuggestions { get; set; }
        //public DbSet<EmployeeSuggestionFile> EmployeeSuggestionFile { get; set; }
        
        public DbSet<UserMaster> UserMaster { get; set; }
        //public DbSet<LCMS> LCMS { get; set; }
        public DbSet<UserMasterDetails> UserMasterDetails { get; set; }
        //public DbSet<Location> Location { get; set; }
        //public DbSet<Area> Area { get; set; }
        //public DbSet<Business> Business { get; set; }
        //public DbSet<FeedLike> feedLikes { get; set; }
        //public DbSet<FeedComments> feedComments { get; set; }
        //public DbSet<FeedCommentsLikeTable> feedCommentsLikeTable { get; set; }
        //public DbSet<FeedContent> feedContents { get; set; }
        //public DbSet<Feed> Feeds { get; set; }
        //public DbSet<SocialCheckHistory> socialCheckHistory { get; set; }
        
        //public DbSet<ThoughtForDay> ThoughtForDay { get; set; }
        public DbSet<Announcements> Announcements { get; set; }
        public DbSet<Publications> Publications { get; set; }
        //public DbSet<NewsUpdates> NewsUpdates { get; set; }
        public DbSet<MediaLibrary> MediaLibrary { get; set; }
        //public DbSet<InterestingArticles> InterestingArticles { get; set; }
        //public DbSet<InterestingArticleCategory> InterestingArticleCategory { get; set; }
        //public DbSet<PollsManagement> PollsManagement { get; set; }
        //public DbSet<SuggestionsManagement> SuggestionsManagement { get; set; }
        //public DbSet<QuizzesManagement> QuizzesManagement { get; set; }
        //public DbSet<QuizQuestionMaster> QuizQuestionMaster { get; set; }
        //public DbSet<QuizOptionMaster> QuizOptionMaster { get; set; }
        //public DbSet<SurveyManagement> SurveyManagement { get; set; }
        //public DbSet<SurveyQuestion> SurveyQuestion { get; set; }
        //public DbSet<SurveyOption> SurveyOption { get; set; }
        //public DbSet<SurveyOptionNested> SurveyOptionNested { get; set; }
        //public DbSet<PollsResult> PollsResult { get; set; }
        //public DbSet<SurveyResult> SurveyResult { get; set; }
        //public DbSet<SurveyResultDetail> SurveyResultDetail { get; set; }
        //public DbSet<QuizResult> QuizResult { get; set; }
        //public DbSet<QuizResultDetail> QuizResultDetail { get; set; }
        public DbSet<MediaLibraryAlbum> MediaLibraryAlbum { get; set; }
        public DbSet<MyAnnouncement> MyAnnouncement { get; set; }
        //public DbSet<MySuggestion> MySuggestion { get; set; }
        //public DbSet<MySuggestionDetail> MySuggestionDetail { get; set; }
        //public DbSet<ThoughtForDayCounter> ThoughtForDayCounter { get; set; }
        //public DbSet<SurveyConfiguration> SurveyConfiguration { get; set; }
        //public DbSet<PollsManagementAccessibilityRule> PollsManagementAccessibilityRule { get; set; }
        //public DbSet<SurveyManagementAccessibilityRule> SurveyManagementAccessibilityRule { get; set; }
        //public DbSet<QuizManagementAccessibilityRule> QuizManagementAccessibilityRule { get; set; }
        public DbSet<OrganizationMessages> OrganizationMessages { get; set; }
        //public DbSet<SurveyQuestionRejected> SurveyQuestionRejected { get; set; }
        //added for token expiry
        public DbSet<Tokens> Tokens { get; set; }
       
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }
        public DbSet<ProjectMaster> ProjectMaster { get; set; }
        public DbSet<ProjectTeamDetails> ProjectTeamDetails { get; set; }
        public DbSet<ProjectApplicationDetails> ProjectApplicationDetails { get; set; }
        public DbSet<IdeaAssignJury> IdeaAssignJuries { get; set; }
        public DbSet<IdeaApplicationJuryAssocation> IdeaApplicationJuryAssocation { get; set; }
        public DbSet<ConfigurableParameter> ConfigurableParameter { get; set; }
        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Gadget");

          //  modelBuilder.Entity<Gadget.API.Models.QuizQuestionMaster>()
          //      .HasOne(p => p.QuizzesManagements)
          //      .WithMany(b => b.QuizQuestionMasters)
          //      .HasForeignKey(p => p.QuizId);

          //  modelBuilder.Entity<Gadget.API.Models.QuizOptionMaster>()
          //      .HasOne(p => p.QuizQuestionMasters)
          //      .WithMany(b => b.QuizOptionMasters)
          //      .HasForeignKey(p => p.QuizQuestionId);

          //  modelBuilder.Entity<Gadget.API.Models.SurveyResultDetail>()
          // .HasOne(p => p.SurveyResults)
          // .WithMany(b => b.SurveyResultDetails)
          // .HasForeignKey(p => p.SurveyResultId);

          //  modelBuilder.Entity<Gadget.API.Models.QuizResultDetail>()
          //.HasOne(p => p.QuizResults)
          //.WithMany(b => b.QuizResultDetails)
          //.HasForeignKey(p => p.QuizResultId);

          //  modelBuilder.Entity<Gadget.API.Models.MySuggestionDetail>()
          //.HasOne(p => p.MySuggestions)
          //.WithMany(b => b.MySuggestionDetails)
          //.HasForeignKey(p => p.SuggestionId);

           modelBuilder.Entity<ProductCategory>().ToTable("ProductCategory", "Profab");
            modelBuilder.Entity<Product>().ToTable("Product", "Profab");
           modelBuilder.Entity<Feature>().ToTable("Feature", "Profab");
           modelBuilder.Entity<Advantage>().ToTable("Advantage", "Profab");
           modelBuilder.Entity<Benefit>().ToTable("Benefit", "Profab");
           modelBuilder.Entity<Image>().ToTable("Image", "Profab");
           modelBuilder.Entity<ProductAccessibility>().ToTable("ProductAccessibility", "Profab");

          //  modelBuilder.Entity<Feed>().ToTable("FeedTable", "WallFeed");
          //  modelBuilder.Entity<FeedContent>().ToTable("FeedContentTable", "WallFeed");
          //  modelBuilder.Entity<FeedLike>().ToTable("FeedLikeTable", "WallFeed");
          //  modelBuilder.Entity<FeedComments>().ToTable("FeedComments", "WallFeed");
          //  modelBuilder.Entity<FeedCommentsLikeTable>().ToTable("FeedCommentsLikeTable", "WallFeed");

          modelBuilder.Entity<ConfigurableParameter>().ToTable("ConfigurableParameter", "Masters");
        }
    }

    public static class DbContextFactory
    {
        public static Dictionary<string, string> ConnectionStrings { get; set; }

        public static void SetConnectionString(Dictionary<string, string> connStrs)
        {
            ConnectionStrings = connStrs;
        }

        public static GadgetDbContext Create(string connectionString)
        {
            DbContextOptionsBuilder<GadgetDbContext> optionsBuilder = new DbContextOptionsBuilder<GadgetDbContext>();
            optionsBuilder.UseSqlServer(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new GadgetDbContext(optionsBuilder.Options);
        }
    }
}
