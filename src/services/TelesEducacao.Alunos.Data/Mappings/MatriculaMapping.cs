using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelesEducacao.Alunos.Domain;

namespace TelesEducacao.Alunos.Data.Mappings;

public class MatriculaMapping : IEntityTypeConfiguration<Matricula>
{
    public void Configure(EntityTypeBuilder<Matricula> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(p => p.Status).HasDefaultValue(MatriculaStatus.PendentePagamento);

        builder.HasOne(c => c.Aluno)
            .WithMany(a => a.Matriculas)
            .HasForeignKey(c => c.AlunoId);
    }
}