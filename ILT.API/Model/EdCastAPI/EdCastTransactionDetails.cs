using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.EdCastAPI
{
    public class EdCastTransactionDetails 
    {
        public int Id { get; set; }
       
        public string RequestUrl { get; set; }
      
        public string TransactionID { get; set; }
       public string External_Id { get; set; }
        public string Payload { get; set; }
       
        public string Http_method { get; set; }
        public string Tran_Status { get; set; }
      
        public string ResponseMessage { get; set; }
        
        public int CreatedBy { get; set; }       
        public DateTime CreatedDate { get; set; }
    }

   
}
