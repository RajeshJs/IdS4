﻿using RajsLibs.Abstraction.Key;
using RajsLibs.Abstraction.Repositories.Operations;
using System;

namespace RajsLibs.Abstraction.Repositories
{
    public interface ICmdRepository<TEntity, TKey> :
        IAdd<TEntity, TKey>,
        IUpdate<TEntity, TKey>,
        IDelete<TEntity, TKey>

        where TEntity : class, IKey<TKey>
        where TKey : IEquatable<TKey>
    {

    }

    public interface ICmdAsyncRepository<TEntity, TKey> :
        IAddAsync<TEntity, TKey>,
        IUpdateAsync<TEntity, TKey>,
        IDeleteAsync<TEntity, TKey>

        where TEntity : class, IKey<TKey>
        where TKey : IEquatable<TKey>
    {

    }
}
