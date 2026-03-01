using FluentValidation.Results;

namespace TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

public class ResponseMessage : Message
{
	public ValidationResult ValidationResult { get; set; }

	public ResponseMessage() { }

	public ResponseMessage(ValidationResult validationResult)
	{
		ValidationResult = validationResult;
	}

	public ResponseMessage(bool success, string errorMessage = null)
	{
		if (success)
		{
			ValidationResult = new ValidationResult();
		}
		else
		{
			var failures = new List<ValidationFailure>
			{
				new ValidationFailure("Error", errorMessage ?? "Operação inválida")
			};
			ValidationResult = new ValidationResult(failures);
		}
	}
}
