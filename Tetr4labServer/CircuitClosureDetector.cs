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

    /// <summary>切断時処理</summary>
    public event Action<string>? Disconnected;

    /// <summary>切断時処理</summary>
    /// <param name="circuit">回路</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">サービス登録時のスコープが誤っている可能性</exception>
    public override Task OnCircuitClosedAsync (Circuit circuit, CancellationToken cancellationToken) {
        Disconnected?.Invoke (circuit.Id);
        if (Disconnected is not null) {
            foreach (Action<string> e in Disconnected.GetInvocationList ()) {
                Disconnected -= e;
            }
        }
        return base.OnCircuitClosedAsync (circuit, cancellationToken);
    }
}
