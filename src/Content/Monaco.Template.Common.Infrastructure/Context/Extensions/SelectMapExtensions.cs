﻿using System.Linq.Expressions;
using DelegateDecompiler.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Monaco.Template.Common.Infrastructure.Context.Extensions;

public static class SelectMapExtensions
{
    public static async Task<TDto?> SingleOrDefaultMapAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Where(predicate).Select(selector).DecompileAsync().SingleOrDefaultAsync(cancellationToken);
    }

    public static async Task<TDto?> SingleOrDefaultMapAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Select(selector).DecompileAsync().SingleOrDefaultAsync(cancellationToken);
    }

    public static async Task<TDto?> FirstOrDefaultMapAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Where(predicate).Select(selector).DecompileAsync().FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<TDto?> SingleOrDefaultAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Where(predicate).Select(selector).SingleOrDefaultAsync(cancellationToken);
    }

    public static async Task<TDto?> FirstOrDefaultMapAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Select(selector).DecompileAsync().FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<TDto?> FirstOrDefaultAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, bool>> predicate, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Where(predicate).Select(selector).FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<List<TDto>> ToListMapAsync<T, TDto>(this IQueryable<T> source, Expression<Func<T, TDto>> selector, CancellationToken cancellationToken = default)
    {
        return await source.Select(selector).DecompileAsync().ToListAsync(cancellationToken);
    }
}