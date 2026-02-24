using System.ComponentModel.DataAnnotations;

namespace TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

public class ResponseMessage : Message
{
	public ValidationResult ValidationResult { get; set; }

	public ResponseMessage(ValidationResult validationResult)
	{
		ValidationResult = validationResult;
	}
}
