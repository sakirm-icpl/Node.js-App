using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assessment.API.Model.EdCastAPI
{
    [Table("DarwinboxTransactionDetails", Schema = "Course")]

    public class DarwinboxTransactionDetails
    {
        public int Id { get; set; }

        public string? RequestUrl { get; set; }


        public string? External_Id { get; set; }
        public string? Payload { get; set; }

        public string? Http_method { get; set; }
        public string? Tran_Status { get; set; }

        public string? ResponseMessage { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }


}
