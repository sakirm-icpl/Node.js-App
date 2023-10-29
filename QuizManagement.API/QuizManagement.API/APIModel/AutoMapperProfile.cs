// ======================================
// <copyright file="AutoMapperProfile.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using AutoMapper;
using QuizManagement.API.APIModel;
using QuizManagement.API.Models;

namespace QuizManagement.API.APIModel
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            //CreateMap<ThoughtForDay, APIThoughtForDay>()
            //      .ReverseMap();
            //CreateMap<Announcements, APIAnnouncements>()
            //      .ReverseMap();
            //CreateMap<Publications, APIPublications>()
            //      .ReverseMap();
            //CreateMap<NewsUpdates, APINewsUpdates>()
            //      .ReverseMap();
            //CreateMap<MediaLibrary, APIMediaLibrary>()
            //     .ReverseMap();
            //CreateMap<InterestingArticles, APIInterestingArticles>()
            //    .ReverseMap();
            //CreateMap<PollsManagement, APIPollsManagement>()
            // .ReverseMap();
            //CreateMap<SuggestionsManagement, APISuggestionsManagement>()
            // .ReverseMap();
            CreateMap<QuizzesManagement, APIQuizzesManagement>()
            .ReverseMap();
            CreateMap<QuizQuestionMaster, APIQuizQuestionMaster>()
            .ReverseMap();
            CreateMap<QuizOptionMaster, APIQuizOptionMaster>()
            .ReverseMap();
            //   CreateMap<SurveyManagement, APISurveyManagement>()
            //   .ReverseMap();
            //   CreateMap<SurveyQuestion, APISurveyQuestion>()
            //   .ReverseMap();
            //   CreateMap<SurveyOption, APISurveyOption>()
            //   .ReverseMap();
            //   CreateMap<SurveyResult, APISurveyResult>()
            //  .ReverseMap();
            //   CreateMap<SurveyResultDetail, APISurveyResultDetail>()
            //  .ReverseMap();
            //   CreateMap<MediaLibraryAlbum, APIMediaLibraryAlbum>()
            //  .ReverseMap();
            //   CreateMap<InterestingArticleCategory, APIInterestingArticleCategory>()
            //  .ReverseMap();
            //   CreateMap<MyAnnouncement, APIMyAnnouncement>()
            // .ReverseMap();
            //   CreateMap<MySuggestion, APIMySuggestion>()
            // .ReverseMap();
            //   CreateMap<ThoughtForDayCounter, APIThoughtForDayCounter>()
            //.ReverseMap();
           // createmap<projectmaster, apiprojectmaster>()
          //.reversemap();
            //   CreateMap<IdeaAssignJury, APIIdeaAssignIdea>()
            // .ReverseMap();
            //   CreateMap<DigitalAdoptionReview, APIDigitalAdoptionReview>()
            // .ReverseMap();
            //   CreateMap<SuggestionCategory, APISuggestionCategory>()
            // .ReverseMap();
            //   CreateMap<AwardList, APIAwardList>()
            // .ReverseMap();
            //   CreateMap<EmployeeAwards, APIEmployeeAwards>()
            // .ReverseMap();
            //   CreateMap<EmployeeSuggestions, APIEmployeeSuggestions>()
            // .ReverseMap();

        }
    }
}
