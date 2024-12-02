using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Tetr4lab;

/// <summary>切断の検出</summary>
/// <example>
/// Program.cs
///   builder.Services.AddScoped&lt;CircuitHandler, CircuitClosureDetector&gt; ();
/// Xxxxx.razor
///   @inject CircuitHandler CircuitHandler
///   
///   @code{
///     protected override async Task OnAfterRenderAsync (bool firstRender) {
///       await base.OnAfterRenderAsync (firstRender);
///   	  if (firstRender &amp;&amp; CircuitHandler is CircuitClosureDetector handler) {
///   	    handler.Disconnected += circuitId => {
///   	      System.Diagnostics.Debug.WriteLine ($"Page Closed {circuitId}");
///   	    };
///   	  }
///   	}
///   }
/// </example>
public class CircuitClosureDetector : CircuitHandler {

    /// <summary>回路一覧</summary>
    protected static ConcurrentDictionary<string, Circuit> Circuits { get; set; } = new ();

    /// <summary>回路ID</summary>
    protected string? Id { get; set; } = default!;

    /// <summary>切断時処理</summary>
    public event Action<string>? Disconnected;

    /// <summary>接続時処理</summary>
    /// <param name="circuit">回路</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">サービス登録時のスコープが誤っている可能性</exception>
    public override Task OnCircuitOpenedAsync (Circuit circuit, CancellationToken cancellationToken) {
        //System.Diagnostics.Debug.WriteLine ($"open {Id}, {circuit.Id}");
        if (Id is not null && Id != circuit.Id || Circuits.ContainsKey (circuit.Id)) {
            throw new InvalidOperationException ($"already has an Id: {Id}, {circuit.Id}");
        }
        Id = circuit.Id;
        Circuits [Id] = circuit;
        return base.OnCircuitOpenedAsync (circuit, cancellationToken);
    }

    /// <summary>切断時処理</summary>
    /// <param name="circuit">回路</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">サービス登録時のスコープが誤っている可能性</exception>
    public override Task OnCircuitClosedAsync (Circuit circuit, CancellationToken cancellationToken) {
        //System.Diagnostics.Debug.WriteLine ($"close {Id}, {circuit.Id}");
        if (Id is null || Id != circuit.Id || !Circuits.ContainsKey (circuit.Id)) {
            throw new InvalidOperationException ($"missmatch Id: {Id}, {circuit.Id}");
        }
        Disconnected?.Invoke (circuit.Id);
        if (Disconnected is not null) {
            foreach (Action<string> e in Disconnected.GetInvocationList ()) {
                Disconnected -= e;
            }
        }
        Circuits.TryRemove (circuit.Id, out var removed);
        return base.OnCircuitClosedAsync (circuit, cancellationToken);
    }
}
