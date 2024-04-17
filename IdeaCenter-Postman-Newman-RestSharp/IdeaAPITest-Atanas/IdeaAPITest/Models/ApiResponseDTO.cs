using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdeaAPITest.Models
{
    public  class ApiResponseDTO // от целия content в respons-a мапваме само тази msg и id.
    {
        [JsonPropertyName("msg")]
        public string Msg {  get; set; }

        [JsonPropertyName("id")]
        public string IdeaId{ get; set; }
    }
}
