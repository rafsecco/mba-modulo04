namespace TelesEducacao.Auth.Application.Models
{
    public class AuthResultDto<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string Message { get; private set; }
        public List<string> Errors { get; private set; }

        private AuthResultDto(bool isSuccess, T? data, string message, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public static AuthResultDto<T> Success(T data, string message = "Operação realizada com sucesso")
        {
            return new AuthResultDto<T>(true, data, message);
        }

        public static AuthResultDto<T> Failure(string error)
        {
            return new AuthResultDto<T>(false, default(T), error, new List<string> { error });
        }

        public static AuthResultDto<T> Failure(List<string> errors)
        {
            var message = errors.Count == 1 ? errors[0] : "Múltiplos erros ocorreram";
            return new AuthResultDto<T>(false, default(T), message, errors);
        }
    }
}