using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Loja.Data;
using loja.models;
using loja.services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LojaDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 26))));

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<FornecedorService>();
builder.Services.AddScoped<VendaService>(); 
builder.Services.AddScoped<DepositoService>(); 

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("chave-super-secreta-12345-12345-12345-124234dfsdf"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints de Usuários
app.MapPost("/usuarios", async (Usuario usuario, UsuarioService usuarioService) =>
{
    await usuarioService.AddUsuarioAsync(usuario);
    return Results.Created($"/usuarios/{usuario.Id}", usuario);
});

app.MapGet("/usuarios", async (UsuarioService usuarioService) =>
{
    var usuarios = await usuarioService.GetAllUsuariosAsync();
    return Results.Ok(usuarios);
});

app.MapPost("/login", async (HttpContext context, UsuarioService usuarioService) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        var json = JsonDocument.Parse(body);

        if (json.RootElement.TryGetProperty("email", out var emailElement) &&
            json.RootElement.TryGetProperty("senha", out var senhaElement))
        {
            var email = emailElement.GetString();
            var senha = senhaElement.GetString();

            if (await usuarioService.ValidateCredentialsAsync(email, senha))
            {
                var token = GenerateToken(email);
                await context.Response.WriteAsync(token);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Credenciais inválidas");
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Corpo da requisição inválido. Email e senha são obrigatórios.");
        }
    }
    catch (JsonException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Falha ao ler o JSON do corpo da requisição.");
    }
});

app.MapGet("/rotaSegura", async (HttpContext context) =>
{
    if (!context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Token não fornecido");
        return;
    }

    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("chave-super-secreta-12345abcdefghijklmnopzdsfytzsdfgzsdyfhuszdf");
    var validationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    SecurityToken validateToken;
    try
    {
        tokenHandler.ValidateToken(token, validationParameters, out validateToken);
        await context.Response.WriteAsync("Autorizado");
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Token inválido");
    }
});

// Endpoints de Clientes
app.MapPost("/clientes", async (Cliente cliente, ClienteService clienteService) =>
{
    await clienteService.AddClienteAsync(cliente);
    return Results.Created($"/clientes/{cliente.Id}", cliente);
});

app.MapGet("/clientes", async (ClienteService clienteService) =>
{
    var clientes = await clienteService.GetAllClientesAsync();
    return Results.Ok(clientes);
});

app.MapGet("/clientes/{id}", async (int id, ClienteService clienteService) =>
{
    var cliente = await clienteService.GetClienteByIdAsync(id);
    if (cliente == null)
    {
        return Results.NotFound($"Cliente with ID {id} not found.");
    }
    return Results.Ok(cliente);
});

// Endpoints de Produtos
app.MapGet("/produtos", async (ProductService productService) =>
{
    var produtos = await productService.GetAllProductsAsync();
    return Results.Ok(produtos);
});

app.MapGet("/produtos/{id}", async (int id, ProductService productService) =>
{
    var produto = await productService.GetProductByIdAsync(id);
    if (produto == null)
    {
        return Results.NotFound($"Product with ID {id} not found.");
    }
    return Results.Ok(produto);
});

app.MapPost("/produtos", async (Produto produto, ProductService productService) =>
{
    await productService.AddProductAsync(produto);
    return Results.Created($"/produtos/{produto.Id}", produto);
});

app.MapPut("/produtos/{id}", async (int id, Produto produto, ProductService productService) =>
{
    if (id != produto.Id)
    {
        return Results.BadRequest("Product ID mismatch.");
    }

    await productService.UpdateProductAsync(produto);
    return Results.Ok();
});

app.MapDelete("/produtos/{id}", async (int id, ProductService productService) =>
{
    await productService.DeleteProductAsync(id);
    return Results.Ok();
});

// Endpoints de Fornecedores
app.MapPost("/fornecedores", async (Fornecedor fornecedor, FornecedorService fornecedorService) =>
{
    await fornecedorService.AddFornecedorAsync(fornecedor);
    return Results.Created($"/fornecedores/{fornecedor.Id}", fornecedor);
});

app.MapGet("/fornecedores", async (FornecedorService fornecedorService) =>
{
    var fornecedores = await fornecedorService.GetAllFornecedoresAsync();
    return Results.Ok(fornecedores);
});

app.MapGet("/fornecedores/{id}", async (int id, FornecedorService fornecedorService) =>
{
    var fornecedor = await fornecedorService.GetFornecedorByIdAsync(id);
    if (fornecedor == null)
    {
        return Results.NotFound($"Fornecedor with ID {id} not found.");
    }
    return Results.Ok(fornecedor);
});

app.MapPut("/fornecedores/{id}", async (int id, Fornecedor fornecedor, FornecedorService fornecedorService) =>
{
    if (id != fornecedor.Id)
    {
        return Results.BadRequest("Fornecedor ID mismatch.");
    }

    await fornecedorService.UpdateFornecedorAsync(fornecedor);
    return Results.Ok();
});

app.MapDelete("/fornecedores/{id}", async (int id, FornecedorService fornecedorService) =>
{
    await fornecedorService.DeleteFornecedorAsync(id);
    return Results.Ok();
});

// Endpoints de Vendas
app.MapPost("/vendas", async (Venda venda, VendaService vendaService) =>
{
    await vendaService.AddVendaAsync(venda);
    return Results.Created($"/vendas/{venda.Id}", venda);
});

app.MapGet("/vendas/produto/{produtoId}/detalhada", async (int produtoId, VendaService vendaService) =>
{
    var vendas = await vendaService.GetVendasPorProdutoDetalhadaAsync(produtoId);
    return Results.Ok(vendas);
});

app.MapGet("/vendas/produto/{produtoId}/sumarizada", async (int produtoId, VendaService vendaService) =>
{
    var vendasSumarizadas = await vendaService.GetVendasPorProdutoSumarizadaAsync(produtoId);
    return Results.Ok(vendasSumarizadas);
});

app.MapGet("/vendas/cliente/{clienteId}/detalhada", async (int clienteId, VendaService vendaService) =>
{
    var vendas = await vendaService.GetVendasPorClienteDetalhadaAsync(clienteId);
    return Results.Ok(vendas);
});

app.MapGet("/vendas/cliente/{clienteId}/sumarizada", async (int clienteId, VendaService vendaService) =>
{
    var vendasSumarizadas = await vendaService.GetVendasPorClienteSumarizadaAsync(clienteId);
    return Results.Ok(vendasSumarizadas);
});

// Endpoints de Depósitos
app.MapPost("/depositos", async (Deposito deposito, DepositoService depositoService) =>
{
    await depositoService.AddDepositoAsync(deposito);
    return Results.Created($"/depositos/{deposito.Id}", deposito);
});

app.MapGet("/depositos", async (DepositoService depositoService) =>
{
    var depositos = await depositoService.GetAllDepositosAsync();
    return Results.Ok(depositos);
});

app.MapGet("/depositos/{id}", async (int id, DepositoService depositoService) =>
{
    var deposito = await depositoService.GetDepositoByIdAsync(id);
    if (deposito == null)
    {
        return Results.NotFound($"Deposito with ID {id} not found.");
    }
    return Results.Ok(deposito);
});

app.MapGet("/depositos/{depositoId}/produtos", async (int depositoId, DepositoService depositoService) =>
{
    var produtos = await depositoService.GetProdutosNoDepositoAsync(depositoId);
    return Results.Ok(produtos);
});

app.MapGet("/produtos/{produtoId}/deposito", async (int produtoId, DepositoService depositoService) =>
{
    var quantidade = await depositoService.GetQuantidadeProdutoNoDepositoAsync(produtoId);
    return Results.Ok(quantidade);
});

// Endpoint para associar um produto a um depósito
app.MapPost("/depositos/{depositoId}/produtos/{produtoId}", async (int depositoId, int produtoId, DepositoService depositoService, ProductService productService) =>
{
    try
    {
        var deposito = await depositoService.GetDepositoByIdAsync(depositoId);
        if (deposito == null)
        {
            return Results.NotFound($"Depósito with ID {depositoId} not found.");
        }

        var produto = await productService.GetProductByIdAsync(produtoId);
        if (produto == null)
        {
            return Results.NotFound($"Product with ID {produtoId} not found.");
        }

        var produtoDeposito = new ProdutoDeposito
        {
            DepositoId = depositoId,
            ProdutoId = produtoId,
            Quantidade = 1 
        };

        await depositoService.DepositProdutoAsync(depositoId, produtoDeposito);

        return Results.Created($"/depositos/{depositoId}/produtos/{produtoId}", produto);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound($"Depósito or Product not found.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");

        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
});

app.Run();

string GenerateToken(string email)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes("chave-super-secreta-12345-12345-12345-124234dfsdf");
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, email)
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}
