using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using RpgApi.Data;
using RpgApi.Models;
using RpgApi.Utils;
using System.Text.RegularExpressions;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly DataContext _context;
        public UsuarioController(DataContext context) { _context = context; }

        public async Task<bool> UsuarioExistente (string username) 
        {
            if (await _context.Usuarios.AnyAsync(
                x => x.Username.ToLower() == username.ToLower())) {
                    
                    return (true);
            } 
            return (false);
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarUsuario(Usuario user ) {
            try {
                if(await UsuarioExistente(user.Username))
                    throw new System.Exception("Nome de usuário já existe");

                Criptografia.CriarPasswordHash(user.PasswordString, out byte[] hash, out byte[] salt);
                user.PasswordString = string.Empty; //empty deixa a string vazia
                user.PasswordHash   = hash;
                user.PasswordSalt   = salt;
                await _context.Usuarios.AddAsync(user);
                await _context.SaveChangesAsync(); //essa função é responsavel por salvar no banco de dados

                return Ok(user.Id);
            }catch (System.Exception ex) {
                return BadRequest(ex.Message);
            }
        } 
       [HttpPost("Autenticar")]
       public async Task<IActionResult> AutenticarUsuario(Usuario credenciais) {
        try {
            Usuario usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(credenciais.Username.ToLower()));

            if(usuario == null) {
                throw new System.Exception("Usuário não encontrado"); // throw espera que aconteça um erro, se o mesmo for detectado ele pula para o CATCH
            }
            else if (!Criptografia.VerificarPasswordHash(credenciais.PasswordString, usuario.PasswordHash, usuario.PasswordSalt)) {
                throw new System.Exception("Senha incorreta");
            }
            else {
                return Ok(usuario.Id);
            }
        }
        catch (System.Exception ex)
        {
            return BadRequest (ex.Message);
        }

       }

       public async Task<IActionResult> Add (Armas novaArma) {
        try {
            if (novaArma.Dano == 0)
                throw new System.Exception ("O dano da arma não pode ser igual a zero.");

            Personagem p = await _context.Personagens
                .FirstOrDefaultAsync ( x => x.Id == novaArma.PersonagemId );

            if ( p == null)
                throw new System.Exception ( "Não exixte nenhum personagem com esse id." );
        
            await _context.Armas.AddAsync(novaArma);
            await _context.SaveChangesAsync();

            return Ok( novaArma.PersonagemId );
        }
        catch (System.Exception ex) {
            return BadRequest (ex.Message);
        }
       }

    //    [HttpPut("AlterarSenha")]
    //    public async Task<IActionResult> AlterarSenha (string NovoPassword, int id) {
    //    }

        [HttpGet("ListarUsuarios")]
        public async Task<IActionResult> ListarUsuarios() {
            try {
                List<Usuario> TodosUsuarios = _context.Usuarios.ToList();

                return Ok (TodosUsuarios);
            }
            catch (System.Exception ex) {
                return BadRequest (ex.Message);
            }
        }
    }
}