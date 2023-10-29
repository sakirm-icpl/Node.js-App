using System;
using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class HouseMaster
    {
        public int Id { get; set; }

        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }
        public int IsDeleted { get; set; }

        [MaxLength(100)]
        public string LogoName { get; set; }

    }
    public class RewardPointsUser : CommonFields
    {
        public int Id { get; set; }
      
        public int TotalPoint { get; set; }
        public int UserId { get; set; }
        public int Rank { get; set; }
    }
}
