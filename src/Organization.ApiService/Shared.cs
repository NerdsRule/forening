global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.AspNetCore.OpenApi;
global using System;
global using System.Net;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using Organization.Shared.DatabaseObjects;
global using Organization.Shared.Interfaces;
global using Organization.Shared.Identity;
global using Organization.Infrastructure.SqlDb;
global using Microsoft.AspNetCore.Identity;

namespace Organization.ApiService;

