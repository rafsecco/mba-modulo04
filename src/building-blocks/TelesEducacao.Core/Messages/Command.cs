using System.Text.Json.Serialization;
using FluentValidation.Results;
using MediatR;

namespace TelesEducacao.Core.Messages;

public abstract class Command : Message, IRequest<bool>
{
    [JsonIgnore]
    public DateTime Timestamp { get; private set; } = DateTime.Now;

	[JsonIgnore]
    public ValidationResult ValidationResult { get; set; }
	public virtual bool EhValido()
	{
		throw new NotImplementedException();
	}
}
