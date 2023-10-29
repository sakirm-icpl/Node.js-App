//======================================
// <copyright file="UserDbContext.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
//======================================
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Payment.API.APIModel;
using Payment.API.Helper;
using Payment.API.Helper.Log_API_Count;
using Payment.API.Models;
using System.Collections.Generic;

//using Payment.API.Models.MyLearningSystem;

namespace Payment.API.Data
{
    public class UserDbContext : DbContext
    {
        private IHttpContextAccessor _httpContext;
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }
        public UserDbContext(DbContextOptions<UserDbContext> options,
            IHttpContextAccessor httpContext)
            : base(options)
        {
            this._httpContext = httpContext;
        }
        public DbSet<UserSettings> UserSettings { get; set; }
        //public DbSet<UserHistory> UserHistory { get; set; }
        //public DbSet<Roles> Roles { get; set; }
        //public DbSet<RoleAuthority> RoleAuthority { get; set; }
        //public DbSet<PermissionMaster> PermissionMaster { get; set; }
        //public DbSet<Location> Location { get; set; }
        //public DbSet<BusinessDetails> BusinessDetails { get; set; }
        //public DbSet<Business> Business { get; set; }
        //public DbSet<Area> Area { get; set; }
        //public DbSet<Group> Group { get; set; }
        //public DbSet<HRMS> Hrms { get; set; }
        //public DbSet<Signup> Signup { get; set; }
        //public DbSet<Configure1> Configure1 { get; set; }
        //public DbSet<Configure2> Configure2 { get; set; }
        //public DbSet<Configure3> Configure3 { get; set; }
        //public DbSet<Configure4> Configure4 { get; set; }
        //public DbSet<Configure5> Configure5 { get; set; }
        //public DbSet<Configure6> Configure6 { get; set; }
        //public DbSet<Configure7> Configure7 { get; set; }
        //public DbSet<Configure8> Configure8 { get; set; }
        //public DbSet<Configure9> Configure9 { get; set; }
        //public DbSet<Configure10> Configure10 { get; set; }
        //public DbSet<Configure11> Configure11 { get; set; }
        //public DbSet<Configure12> Configure12 { get; set; }
        //public DbSet<Configure13> Configure13 { get; set; }
        //public DbSet<Configure14> Configure14 { get; set; }
        //public DbSet<Configure15> Configure15 { get; set; }


        //public DbSet<JobResponsibility> JobResponsibility { get; set; }
        //public DbSet<KeyAreaSetting> KeyAreaSetting { get; set; }
        //public DbSet<JobAid> JobAid { get; set; }
        //public DbSet<MailTemplateDesigner> MailTemplateDesigner { get; set; }
        //public DbSet<JobResponsibilityDetail> JobResponsibilityDetail { get; set; }
        //public DbSet<UserMasterRejected> UserMasterRejected { get; set; }
        //public DbSet<MyPreferences> MyPreferences { get; set; }
        //public DbSet<FunctionsMaster> FunctionsMaster { get; set; }
        public DbSet<CustomerConnectionString> CustomerConnectionString { get; set; }
        //public DbSet<AppModule> AppModule { get; set; }
        public DbSet<UserMaster> UserMaster { get; set; }
        public DbSet<UserMasterDetails> UserMasterDetails { get; set; }
        //public DbSet<PasswordHistory> PasswordHistory { get; set; }
        //public DbSet<LoggedInHistory> LoggedInHistory { get; set; }
        //public DbSet<UserMasterOtp> UserMasterOtp { get; set; }
        //public DbSet<UserLoginStatistics> UserLoginStatistics { get; set; }
        //public DbSet<UserLoginStatiticsForRewardPoints> UserLoginStatiticsForRewardPoints { get; set; }
        //public DbSet<FailedLoginHistory> FailedLoginHistory { get; set; }
        //public DbSet<FailedLoginStatistics> FailedLoginStatistics { get; set; }
        //public DbSet<UserHRAssociation> UserHRAssociation { get; set; }
        public DbSet<UserRejectedLog> UserRejectedLog { get; set; }
        //public DbSet<UserAuthOtp> UserAuthOtp { get; set; }
        //public DbSet<Demo> Demo { get; set; }
        //public DbSet<UserMasterOtpHistory> UserMasterOtpHistory { get; set; }
        //public DbSet<ExportFileLog> ExportFileLog { get; set; }
        //public DbSet<HouseMaster> HouseMaster { get; set; }
        //public DbSet<UserTrainingAdminAssociation> UserTrainingAdminAssociation { get; set; }
        //public DbSet<OrganizationPreferences> OrganizationPreferences { get; set; }
        //public DbSet<TokenBlacklist> TokenBlacklist { get; set; }
        //public DbSet<Tokens> Tokens { get; set; }
        //public DbSet<DesignationRoleMapping> DesignationRoleMapping { get; set; }
        //public DbSet<UserMasterDelete> UserMasterDelete { get; set; }
        //public DbSet<UserMasterDetailsDelete> UserMasterDetailsDelete { get; set; }

        //public DbSet<UserRejectedStatus> UserRejectedStatus { get; set; }
        //public DbSet<SignUpOTP> SignUpOTP { get; set; }

        //public DbSet<FCMToken> FCMToken { get; set; }
        //public DbSet<MyPreferenceConfiguration> MyPreferenceConfigurations { get; set; }
        //public DbSet<SocialMediaRule> SocialMediaRule { get; set; }
        //public DbSet<CustomizeNotificationImportedUsers> CustomizeNotificationImportedUsers { get; set; }
        //public DbSet<SocialMediaRejected> SocialMediaRejected { get; set; }
        public DbSet<UserSignUp> UserSignUp { get; set; }
        //public DbSet<UserMasterLogs> UserMasterLogs { get; set; }
        public DbSet<NodalCourseRequests> NodalCourseRequests { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<NodalUserGroups> NodalUserGroups { get; set; }
        //public DbSet<NodalGroupsUsersMapping> NodalGroupsUsersMapping { get; set; }
        //public DbSet<GroupCode> GroupCode { get; set; }
        public DbSet<AccessibilityRule> AccessibilityRule { get; set; }
        public DbSet<PaymentResponse> PaymentResponse { get; set; }
        //public DbSet<ConfigurableValues> ConfigurableValues { get; set; }
        //public DbSet<UserWebinarMaster> UserWebinarMaster { get; set; }
        //public DbSet<UserTeams> UserTeams { get; set; }
        //public DbSet<UserTeamsMapping> UserTeamsMapping { get; set; }
        //public DbSet<BasicAuthCredentials> BasicAuthCredentials { get; set; }
        //public DbSet<Notifications> Notifications { get; set; }
        //public DbSet<ApplicableNotifications> ApplicableNotifications { get; set; }
        public DbSet<ClientUserApiCount> ClientUserApiCount { get; set; }
        public DbSet<TransactionRequest> TransactionRequest { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string ConnectionString = null;
            if (this._httpContext != null)
            {
                var httpContext = this._httpContext.HttpContext;
                if (httpContext != null)
                {
                    string encryptedConnectionString = httpContext.User.FindFirst("address") == null ? null : httpContext.User.FindFirst("address").Value;
                    if (!string.IsNullOrEmpty(encryptedConnectionString))
                    {
                        ConnectionString = Security.Decrypt(encryptedConnectionString);
                        optionsBuilder
                        .UseSqlServer(ConnectionString, opt => opt.UseRowNumberForPaging())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                        base.OnConfiguring(optionsBuilder);
                    }
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("User");

            modelBuilder.Entity<UserMaster>().Property(x => x.RowGuid).HasDefaultValueSql("NEWID()");
            //modelBuilder.Entity<ConfigurableValues>().ToTable("ConfigurableValues", "Masters");
            //modelBuilder.Entity<Notifications>().ToTable("Notifications", "Notification");
            //modelBuilder.Entity<ApplicableNotifications>().ToTable("ApplicableNotifications", "Notification");
            modelBuilder.Entity<ClientUserApiCount>().ToTable("ClientUserApiCount", "dbo");
        }
    }

    public static class DbContextFactory
    {
        public static Dictionary<string, string> ConnectionStrings { get; set; }

        public static void SetConnectionString(Dictionary<string, string> connStrs)
        {
            ConnectionStrings = connStrs;
        }

        public static UserDbContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            optionsBuilder
            .UseSqlServer(connectionString, opt => opt.UseRowNumberForPaging())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new UserDbContext(optionsBuilder.Options);
        }
    }

}
