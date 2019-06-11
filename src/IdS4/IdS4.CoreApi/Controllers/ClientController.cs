﻿using System;
using AutoMapper;
using IdS4.CoreApi.Models.Client;
using IdS4.CoreApi.Models.Paging;
using IdS4.CoreApi.Models.Results;
using IdS4.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using GrantType = IdentityServer4.Models.GrantType;

namespace IdS4.CoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IdS4ConfigurationDbContext _configurationDb;
        private readonly ILogger<ResourceController> _logger;
        private readonly IMapper _mapper;

        public ClientController(IdS4ConfigurationDbContext configurationDb, ILogger<ResourceController> logger, IMapper mapper)
        {
            _configurationDb = configurationDb;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<Paged<VmClient>> Get([FromQuery]PageQuery query)
        {
            var clients = await _configurationDb.Clients.AsNoTracking()
                .OrderBy(query.Sort ?? "Id")
                .Skip(query.Skip)
                .Take(query.Limit)
                .ToListAsync();

            return Paged<VmClient>.From(
                _mapper.Map<List<VmClient>>(clients),
                await _configurationDb.Clients.AsNoTracking().CountAsync()
            );
        }

        [HttpGet("{id}")]
        public async Task<ApiResult> Get([FromRoute]int id)
        {
            if (id <= 0) return ApiResult.NotFound(id);

            var apiResult = await GetClient(id);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var vm = (apiResult as ApiResult<VmClient>)?.Data;
            if (vm == null) return ApiResult.Failure();

            return ApiResult.Success(new
            {
                Basic = vm.ToBasic(_mapper),
                Authenticate = vm.ToAuthenticate(_mapper),
                Token = vm.ToToken(_mapper),
                Consent = vm.ToConsent(_mapper),
                Device = vm.ToDevice(_mapper)
            });
        }

        [HttpPost]
        public async Task<ApiResult> Add([FromBody]VmClientAdd vm)
        {
            if (!ModelState.IsValid) return ApiResult.Failure(ModelState);

            var vmClient = PrepareClient(vm);

            var apiResult = await ValidateClient(vmClient);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var client = _mapper.Map<Client>(vmClient);
            await _configurationDb.Clients.AddAsync(client);
            await _configurationDb.SaveChangesAsync();
            return ApiResult.Success(_mapper.Map<VmClient>(client));
        }

        [HttpPatch("basic")]
        public async Task<ApiResult> Edit([FromBody]VmClient.Basic vm)
        {
            if (vm.Id <= 0) return ApiResult.NotFound(vm.Id);

            var apiResult = await GetClient(vm.Id);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var vmClient = (apiResult as ApiResult<VmClient>)?.Data;
            if (vmClient == null) return ApiResult.Failure();

            vm.ApplyChangeToClient(vmClient);
            vmClient = await EditClient(vmClient);
            return ApiResult.Success(vmClient.ToBasic(_mapper));
        }

        [HttpPatch("authenticate")]
        public async Task<ApiResult> Edit([FromBody]VmClient.Authenticate vm)
        {
            if (vm.Id <= 0) return ApiResult.NotFound(vm.Id);

            var apiResult = await GetClient(vm.Id);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var vmClient = (apiResult as ApiResult<VmClient>)?.Data;
            if (vmClient == null) return ApiResult.Failure();

            vm.ApplyChangeToClient(vmClient);
            vmClient = await EditClient(vmClient);
            return ApiResult.Success(vmClient.ToAuthenticate(_mapper));
        }

        [HttpPatch("token")]
        public async Task<ApiResult> Edit([FromBody]VmClient.Token vm)
        {
            if (vm.Id <= 0) return ApiResult.NotFound(vm.Id);

            var apiResult = await GetClient(vm.Id);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var vmClient = (apiResult as ApiResult<VmClient>)?.Data;
            if (vmClient == null) return ApiResult.Failure();

            vm.ApplyChangeToClient(vmClient);
            vmClient = await EditClient(vmClient);
            return ApiResult.Success(vmClient.ToToken(_mapper));
        }

        [HttpPatch("consent")]
        public async Task<ApiResult> Edit([FromBody]VmClient.Consent vm)
        {
            if (vm.Id <= 0) return ApiResult.NotFound(vm.Id);

            var apiResult = await GetClient(vm.Id);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var vmClient = (apiResult as ApiResult<VmClient>)?.Data;
            if (vmClient == null) return ApiResult.Failure();

            vm.ApplyChangeToClient(vmClient);
            vmClient = await EditClient(vmClient);
            return ApiResult.Success(vmClient.ToConsent(_mapper));
        }

        [HttpPatch("device")]
        public async Task<ApiResult> Edit([FromBody]VmClient.Device vm)
        {
            if (vm.Id <= 0) return ApiResult.NotFound(vm.Id);

            var apiResult = await GetClient(vm.Id);
            if (apiResult.Code != ApiResultCode.Success) return apiResult;

            var vmClient = (apiResult as ApiResult<VmClient>)?.Data;
            if (vmClient == null) return ApiResult.Failure();

            vm.ApplyChangeToClient(vmClient);
            vmClient = await EditClient(vmClient);
            return ApiResult.Success(vmClient.ToDevice(_mapper));
        }

        [HttpDelete("{ids}")]
        public async Task<ApiResult> Remove([FromRoute]string ids)
        {
            if (string.IsNullOrEmpty(ids)) return ApiResult.Failure();

            foreach (var id in ids.Split(","))
            {
                var entity = await _configurationDb.Clients.FindAsync(int.Parse(id));
                if (entity == null) continue;

                _configurationDb.Clients.Remove(entity);
            }
            await _configurationDb.SaveChangesAsync();
            return ApiResult.Success();
        }

        #region privates

        private VmClient PrepareClient(VmClientAdd vm)
        {
            var client = new VmClient
            {
                ClientId = vm.ClientId,
                ClientName = vm.ClientName
            };

            switch (vm.Type)
            {
                case VmClientType.Empty:
                    break;
                case VmClientType.Hybrid:
                    client.AllowedGrantTypes.Add(new VmClientGrantType(GrantType.Hybrid));
                    break;
                case VmClientType.SPA:
                    client.AllowedGrantTypes.Add(new VmClientGrantType(GrantType.AuthorizationCode));
                    client.RequirePkce = true;
                    client.RequireClientSecret = false;
                    break;
                case VmClientType.Native:
                    client.AllowedGrantTypes.Add(new VmClientGrantType(GrantType.Hybrid));
                    break;
                case VmClientType.Machine:
                    client.AllowedGrantTypes.Add(new VmClientGrantType(GrantType.ResourceOwnerPassword));
                    client.AllowedGrantTypes.Add(new VmClientGrantType(GrantType.ClientCredentials));
                    break;
                case VmClientType.Device:
                    client.AllowedGrantTypes.Add(new VmClientGrantType(GrantType.DeviceFlow));
                    client.RequireClientSecret = false;
                    client.AllowOfflineAccess = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return client;
        }

        private async Task<ApiResult> ValidateClient(VmClient client)
        {
            if (await _configurationDb.Clients.AnyAsync(s => s.ClientId.Equals(client.ClientId)))
            {
                return ApiResult.Failure(errors:
                    new KeyValuePair<string, object>("客户端已存在", $"客户端ID: {client.ClientId} 已存在")
                    );
            }

            return ApiResult.Success();
        }

        private async Task<ApiResult> GetClient(int id)
        {
            var client = await _configurationDb.Clients.AsNoTracking().SingleOrDefaultAsync(s => s.Id == id);
            if (client == null) return ApiResult.NotFound(id);

            client.ClientSecrets =
                await _configurationDb.ClientSecrets.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.AllowedGrantTypes =
                await _configurationDb.ClientGrantTypes.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.RedirectUris =
                await _configurationDb.ClientRedirectUris.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.PostLogoutRedirectUris =
                await _configurationDb.ClientPostLogoutRedirectUris.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.AllowedScopes =
                await _configurationDb.ClientScopes.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.IdentityProviderRestrictions =
                await _configurationDb.ClientIdPRestrictions.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.Claims =
                await _configurationDb.ClientClaims.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.AllowedCorsOrigins =
                await _configurationDb.ClientCorsOrigins.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();
            client.Properties =
                await _configurationDb.ClientProperties.AsNoTracking().Where(s => s.ClientId == client.Id).ToListAsync();

            var vm = _mapper.Map<VmClient>(client);
            return ApiResult.Success(vm);
        }

        private async Task<VmClient> EditClient(VmClient vm)
        {
            var entity = _mapper.Map<Client>(vm);
            var entry = _configurationDb.Attach(entity);
            entry.State = EntityState.Modified;

            await _configurationDb.SaveChangesAsync();
            return _mapper.Map<VmClient>(entity);
        }

        #endregion
    }
}