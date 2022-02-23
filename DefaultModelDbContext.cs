using Microsoft.EntityFrameworkCore;
using System;

namespace Meep.Tech.Data.EFCore {


  /// <summary>
  /// The default db context class for the model serializer.
  /// If you use your own, make sure to call modelBuilder.SetUpEcsbamModels();
  /// </summary>
  public partial class DefaultModelDbContext : DbContext {

    Action<DbContextOptionsBuilder> _onConfiguring {
      get;
    }

    /// <summary>
    /// The universe this is for
    /// </summary>
    Universe _universe {
      get;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public DefaultModelDbContext(DbContextOptions options, Universe universe = null, Action<DbContextOptionsBuilder> onConfiguring = null)
        : base(options) {
      _onConfiguring = onConfiguring;
      _universe = universe ?? Models.DefaultUniverse;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      modelBuilder.SetUpEcsbamModels(_universe);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      base.OnConfiguring(optionsBuilder);
      _onConfiguring?.Invoke(optionsBuilder);
    }
  }
}