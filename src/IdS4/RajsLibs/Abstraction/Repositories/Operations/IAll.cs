﻿using RajsLibs.Abstraction.Key;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RajsLibs.Abstraction.Repositories.Operations
{
    public interface IAll<out TEntity, in TKey>
        where TEntity : class, IKey<TKey>
        where TKey : IEquatable<TKey>
    {
        IEnumerable<TEntity> All();
    }

    public interface IAllAsync<TEntity, in TKey>
        where TEntity : class, IKey<TKey>
        where TKey : IEquatable<TKey>
    {
        Task<IEnumerable<TEntity>> AllAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
