using System.Collections.Generic;

namespace naif_katalog.Models
{
    public class ResponseDto<T>
    {
        public T data { get; set; }
        public bool isSuccess { get; set; }
        public int statusCode { get; set; }
        public List<string> errors { get; set; }

        public ResponseDto<T> Success(T data, int statusCode = 200)
        {
            this.data = data;
            this.isSuccess = true;
            this.statusCode = statusCode;
            return this;
        }

        public ResponseDto<T> Fail(string error, int statusCode = 400)
        {
            this.errors = new List<string> { error };
            this.isSuccess = false;
            this.statusCode = statusCode;
            return this;
        }

        public ResponseDto<T> Fail(List<string> errors, int statusCode = 400)
        {
            this.errors = errors;
            this.isSuccess = false;
            this.statusCode = statusCode;
            return this;
        }
    }
}
