using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelesEducacao.Conteudos.Domain;

namespace TelesEducacao.Conteudos.Data.Mappings;

public class CursoMapping : IEntityTypeConfiguration<Curso>
{
    public void Configure(EntityTypeBuilder<Curso> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(p => p.Nome).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Descricao).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Ativo).HasDefaultValue(true);
        builder.Property(c => c.Valor)
               .HasPrecision(18, 2);

        //transformando o objeto de valor ConteudoProgramatico em colunas na tabela Curso
        builder.OwnsOne(c => c.ConteudoProgramatico, cp =>
        {
            cp.Property(c => c.Descricao)
                .HasColumnName("DescricaoConteudoProgramatico")
                .HasColumnType("varchar(500)");

            cp.Property(c => c.Titulo)
                .HasColumnName("TituloConteudoProgramatico")
                .HasColumnType("varchar(100)");
        });
    }
}