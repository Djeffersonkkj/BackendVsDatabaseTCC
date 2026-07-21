"""
Gera boxplots mais legiveis a partir dos dados brutos do benchmark.

Visualizacao:
- 1 arquivo PNG por operacao
- boxplot horizontal
- eixo de tempo em milissegundos
- pontos brutos sobrepostos para mostrar a distribuicao real
"""

from __future__ import annotations

import math
import random
from pathlib import Path

import matplotlib.pyplot as plt


OUTPUT_DIR = Path(__file__).with_name("boxplots")
random.seed(42)

OPERACOES = [
    {
        "nome": "RegistrarPedido",
        "backend": [33.35, 16.46, 13.65, 15.54, 13.96, 13.69, 16.60, 14.28, 13.89, 12.55],
        "procedure": [2.12, 2.34, 2.05, 2.85, 2.08, 2.67, 2.37, 2.46, 2.56, 2.33],
    },
    {
        "nome": "Relatorio consolidado",
        "backend": [1119.65, 1054.59, 1056.93, 998.75, 1050.56, 1009.35, 1067.28, 1035.41, 1045.35, 1034.68],
        "procedure": [384.12, 390.89, 409.69, 370.29, 377.74, 359.35, 377.08, 392.66, 369.06, 385.71],
    },
    {
        "nome": "Relatorio vendas por periodo",
        "backend": [32.63, 12.42, 12.52, 11.23, 10.44, 11.47, 9.99, 12.92, 10.26, 10.15],
        "procedure": [13.77, 11.96, 10.86, 10.67, 9.23, 9.13, 10.35, 9.61, 9.65, 11.76],
    },
]


def media(valores: list[float]) -> float:
    return sum(valores) / len(valores)


def mediana(valores: list[float]) -> float:
    ordenados = sorted(valores)
    meio = len(ordenados) // 2
    if len(ordenados) % 2:
        return ordenados[meio]
    return (ordenados[meio - 1] + ordenados[meio]) / 2


def quartil(valores: list[float], p: float) -> float:
    ordenados = sorted(valores)
    if len(ordenados) == 1:
        return ordenados[0]

    pos = (p / 100.0) * (len(ordenados) - 1)
    baixo = math.floor(pos)
    alto = math.ceil(pos)
    if baixo == alto:
        return ordenados[baixo]

    peso = pos - baixo
    return ordenados[baixo] + (ordenados[alto] - ordenados[baixo]) * peso


def resumo(valores: list[float]) -> str:
    return (
        f"min={min(valores):.2f} | q1={quartil(valores, 25):.2f} | "
        f"mediana={mediana(valores):.2f} | q3={quartil(valores, 75):.2f} | "
        f"max={max(valores):.2f}"
    )


def espalhar_pontos(valores: list[float], centro: float) -> tuple[list[float], list[float]]:
    xs = valores
    ys = [centro + random.uniform(-0.045, 0.045) for _ in valores]
    return xs, ys


def desenhar_operacao(operacao: dict[str, list[float]]) -> Path:
    nome = operacao["nome"]
    backend = operacao["backend"]
    procedure = operacao["procedure"]

    fig, ax = plt.subplots(figsize=(12, 4.8))

    box = ax.boxplot(
        [backend, procedure],
        vert=False,
        patch_artist=True,
        showmeans=True,
        meanline=True,
        widths=0.55,
    )

    cores = ["#4E79A7", "#F28E2B"]
    for patch, cor in zip(box["boxes"], cores):
        patch.set_facecolor(cor)
        patch.set_alpha(0.78)

    for median in box["medians"]:
        median.set_color("#111111")
        median.set_linewidth(2.2)

    for mean in box["means"]:
        mean.set_color("#111111")
        mean.set_marker("D")
        mean.set_markersize(6)

    for i, serie in enumerate([backend, procedure], start=1):
        xs, ys = espalhar_pontos(serie, i)
        ax.scatter(xs, ys, s=28, alpha=0.42, color="#222222", edgecolors="none", zorder=3)

    ax.set_xlabel("Tempo (ms)")
    ax.grid(axis="x", linestyle="--", alpha=0.28)
    ax.set_yticks([1, 2])
    ax.set_yticklabels(["Backend", "Procedure"])

    valores = backend + procedure
    minimo = min(valores)
    maximo = max(valores)
    largura = maximo - minimo
    margem = max(largura * 0.12, 1.0)
    ax.set_xlim(left=max(minimo - margem, 0), right=maximo + margem)

    footer = (
        f"Backend: {resumo(backend)}\n"
        f"Procedure: {resumo(procedure)}\n"
        f"Media backend: {media(backend):.2f} ms | media procedure: {media(procedure):.2f} ms"
    )

    fig.subplots_adjust(top=0.86, bottom=0.24, left=0.12, right=0.98)
    fig.text(
        0.5,
        0.055,
        footer,
        ha="center",
        va="bottom",
        fontsize=9,
        bbox=dict(boxstyle="round,pad=0.5", facecolor="#f6f6f6", edgecolor="#cccccc"),
    )

    ax.set_title(f"Boxplot de tempos - {nome}", fontsize=15, pad=20)
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    output_path = OUTPUT_DIR / f"{nome.lower().replace(' ', '_')}.png"
    fig.savefig(output_path, dpi=220, bbox_inches="tight")
    plt.close(fig)
    return output_path


def main() -> int:
    paths = [desenhar_operacao(operacao) for operacao in OPERACOES]

    print("Boxplots gerados:")
    for path in paths:
        print(path)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
