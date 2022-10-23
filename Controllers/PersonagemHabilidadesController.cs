using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RpgApi.Models;
using RpgApi.Data;
using System.Threading.Tasks;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class PersonagemHabilidadesController : ControllerBase
    {
        private readonly DataContext _context;
        public PersonagemHabilidadesController (DataContext context) { _context = context; }


        public async Task<IActionResult> AddPersonagemHabilidades (PersonagemHabilidade NovoPersonagemHibilidade) {
            try {
                Personagem personagem = await _context.Personagens
                    .Include(p => p.Armas)
                    .Include(p => p.PersonagemHabilidades).ThenInclude(Ps => Ps.Habilidade)
                    .FirstOrDefaultAsync(p => p.Id == NovoPersonagemHibilidade.PersonagemId);

                if (personagem == null)
                    throw new System.Exception ("Não exixte nenhum jogando com esse id.");

                Habilidade habilidade = await _context.Habilidade
                                        .FirstOrDefaultAsync(h => h.Id == NovoPersonagemHibilidade.HabilidadeId);
                if (habilidade == null) 
                    throw new System.Exception ("Habilidade não encontrada.");

                PersonagemHabilidade ph = new PersonagemHabilidade();
                ph.Personagem = personagem;
                ph.Habilidade = habilidade;
                await _context.PersonagemHabilidades.AddAsync(ph);
                int LinhasAfetadas = await _context.SaveChangesAsync();

                return Ok($"{LinhasAfetadas} linhas afetadas.");
            }
            catch (System.Exception ex) {
                return BadRequest (ex.Message);
            }
        }

        
    }
}