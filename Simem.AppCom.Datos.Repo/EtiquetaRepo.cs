﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simem.AppCom.Datos.Dto;
using Simem.AppCom.Base.Repo;
using Simem.AppCom.Datos.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace Simem.AppCom.Datos.Repo
{
    public class EtiquetaRepo:IEtiquetaRepo
    {
        private readonly DbContextSimem _baseContext;

        public EtiquetaRepo()
        {
            _baseContext = new DbContextSimem();
        }

        public List<EnlaceDto> GetTags()
        {
            List<Datos.Dto.EnlaceDto> lstReturn = new List<EnlaceDto>();
            var dbEntity = _baseContext.Etiqueta.Take(5).ToList();
            foreach (var item in dbEntity)
            {
                EnlaceDto entityDto = new EnlaceDto();
                entityDto.Id = item.Id;
                entityDto.Titulo = item.Titulo;
                entityDto.Estado = item.Estado;
                lstReturn.Add(entityDto);
            }
            return lstReturn;
        }

        public EnlaceDto GetTag(Guid idRegistry)
        {
            var dbEntity = _baseContext.Etiqueta.FirstOrDefault(c => c.Id.Equals(idRegistry));
            EnlaceDto entityDto = new EnlaceDto();
            if (dbEntity != null)
            {
                entityDto.Id = dbEntity.Id;
                entityDto.Titulo = dbEntity.Titulo;
                entityDto.Estado = dbEntity.Estado;
            }

            return entityDto;
        }

        public async Task NewTag(EnlaceDto entityDto)
        {
            var transaction = _baseContext.Database.BeginTransaction();

            Etiqueta dbEntity = new Etiqueta();
            dbEntity.Estado = true;
            dbEntity.Titulo = entityDto.Titulo;
            dbEntity.Id = Guid.NewGuid();
            try
            {
                _baseContext.Etiqueta.Add(dbEntity);
                await _baseContext.SaveChangesAsync();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeleteTag(Guid idRegistry)
        {
            var dbEntity = _baseContext.Etiqueta.FirstOrDefault(c => c.Id.Equals(idRegistry));
            var transaction = _baseContext.Database.BeginTransaction();
            try
            {
                if (dbEntity != null)
                {
                    _baseContext.Etiqueta.Remove(dbEntity);
                    await _baseContext.SaveChangesAsync();
                    transaction.Commit();
                }

            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ModifyTag(EnlaceDto entityDto)
        {
            var transaction = _baseContext.Database.BeginTransaction();
            bool response = false;
            var dbEntity = _baseContext.Etiqueta.FirstOrDefault(x => x.Id.Equals(entityDto.Id));

            try
            {
                if (dbEntity != null)
                {
                    dbEntity.Estado = entityDto.Estado;
                    dbEntity.Titulo = entityDto.Titulo;
                    await _baseContext.SaveChangesAsync();
                    transaction.Commit();
                    return true;
                }

            }
            catch (Exception)
            {
                transaction.Rollback();
            }

            return response;
        }
        ///Get all
        public async Task<List<ConjuntoDatosDto>> GetDatosDto()
        {
            var datos = await (from gae in _baseContext.GeneracionArchivoEtiqueta
                               join e in _baseContext.Etiqueta on gae.EtiquetaId equals e.Id
                               join ga in _baseContext.GeneracionArchivo on gae.IdConfiguracionGeneracionArchivo equals ga.IdConfiguracionGeneracionArchivos
                               select new ConjuntoDatosDto
                               {
                                   Id = gae.IdConfiguracionGeneracionArchivoxEtiqueta,
                                   Titulo = e.Titulo,
                                   Estado = e.Estado,
                                   ConjuntoDeDatosAsociados = ga.Titulo
                               }).ToListAsync();

            return datos;
        }

        //Get Id
        public async Task<List<ConjuntoDatosDto>> GetDatosDtoById(Guid id)
        {
            var datos = await (from gae in _baseContext.GeneracionArchivoEtiqueta
                               join e in _baseContext.Etiqueta on gae.EtiquetaId equals e.Id
                               join ga in _baseContext.GeneracionArchivo on gae.IdConfiguracionGeneracionArchivo equals ga.IdConfiguracionGeneracionArchivos
                               where gae.IdConfiguracionGeneracionArchivoxEtiqueta == id
                               select new ConjuntoDatosDto
                               {
                                   Id = gae.IdConfiguracionGeneracionArchivoxEtiqueta,
                                   Titulo = e.Titulo,
                                   Estado = e.Estado,
                                   ConjuntoDeDatosAsociados = ga.Titulo
                               }).ToListAsync();

            return datos;
        }
        //Delete

        public async Task DeleteDatosById(Guid id)
        {
            // Elimina los registros de GeneracionArchivoEtiqueta de manera asincrónica
            var generacionArchivoEtiquetasAEliminar = await _baseContext.GeneracionArchivoEtiqueta
                .Where(gae => gae.IdConfiguracionGeneracionArchivoxEtiqueta == id)
                .ToListAsync();
            _baseContext.GeneracionArchivoEtiqueta.RemoveRange(generacionArchivoEtiquetasAEliminar);

            // Elimina los registros de Etiqueta de manera asincrónica
            var etiquetasAEliminar = await _baseContext.Etiqueta
                .Where(e => e.Id == id)
                .ToListAsync();
            _baseContext.Etiqueta.RemoveRange(etiquetasAEliminar);

            // Elimina los registros de GeneracionArchivo de manera asincrónica
            var generacionArchivosAEliminar = await _baseContext.GeneracionArchivo
                .Where(ga => ga.IdConfiguracionGeneracionArchivos == id)
                .ToListAsync();

            _baseContext.GeneracionArchivo.RemoveRange(generacionArchivosAEliminar);

            // Guarda los cambios en la base de datos de manera asincrónica
            await _baseContext.SaveChangesAsync();
        }




    }

}
