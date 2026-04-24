using TelesEducacao.Conteudos.Domain;
using TelesEducacao.Core.DomainObjects;

namespace TelesEducacao.Conteudo.Domain.Tests
{
    public class CursoTests
    {
        [Fact]
        public void Curso_Validar_ValidacoesDevemRetornarExceptions()
        {
            // Arrange & Act & Assert

            var ex = Assert.Throws<DomainException>(() =>
                new Curso(string.Empty, "Descricao", false, 100, new ConteudoProgramatico("Conteudo 1", "Descricao conteudo"))
            );

            Assert.Equal("O campo Nome do curso não pode estar vazio", ex.Message);

            ex = Assert.Throws<DomainException>(() =>
                new Curso("Nome", string.Empty, false, 100, new ConteudoProgramatico("Conteudo 1", "Descricao conteudo"))
            );

            Assert.Equal("O campo Descricao do curso não pode estar vazio", ex.Message);

            ex = Assert.Throws<DomainException>(() =>
                new Curso("Nome", "Descricao", false, 0, new ConteudoProgramatico("Conteudo 1", "Descricao conteudo"))
            );

            Assert.Equal("O campo Valor do curso não pode se menor igual a 0", ex.Message);

            ex = Assert.Throws<DomainException>(() =>
                new Curso("Nome", "Descricao", false, 100, new ConteudoProgramatico(string.Empty, "Descricao conteudo"))
            );

            Assert.Equal("O campo Titulo do conteudo programatico não pode estar vazio", ex.Message);

            ex = Assert.Throws<DomainException>(() =>
                new Curso("Nome", "Descricao", false, 100, new ConteudoProgramatico("Conteudo 1", string.Empty))
            );

            Assert.Equal("O campo Descricao da conteudo programatico não pode estar vazio", ex.Message);
        }
    }
}