"use strict";

// --- Referências de DOM ---
const lista = document.getElementById("lista");
const totalCount = document.getElementById("totalCount");
const vencendoCount = document.getElementById("vencendoCount");
const vencendoList = document.getElementById("vencendoList");
const dlg = document.getElementById("dlgNova");
const form = document.getElementById("formNova");
const formError = document.getElementById("formError");

// --- Cliente HTTP mínimo (mesma origem, sem CORS) ---
async function api(method, path, body) {
    const res = await fetch(path, {
        method,
        headers: body ? { "Content-Type": "application/json" } : undefined,
        body: body ? JSON.stringify(body) : undefined,
    });

    if (res.status === 204) return null;

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;

    if (!res.ok) {
        // A API padroniza { erro: "..." } (um endpoint usa { error }).
        const msg = (data && (data.erro || data.error)) || "Não foi possível completar a operação.";
        throw new Error(msg);
    }
    return data;
}

// --- Formatação ---
const brl = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
const money = (v) => brl.format(v ?? 0);

function date(iso) {
    if (!iso) return "—";
    const [y, m, d] = iso.substring(0, 10).split("-");
    return `${d}/${m}/${y}`;
}

function doc(value) {
    const s = String(value || "").replace(/\D/g, "");
    if (s.length === 11) return s.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
    if (s.length === 14) return s.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, "$1.$2.$3/$4-$5");
    return value;
}

function diasAte(iso) {
    const fim = new Date(iso.substring(0, 10) + "T00:00:00");
    const hoje = new Date();
    hoje.setHours(0, 0, 0, 0);
    return Math.max(0, Math.round((fim - hoje) / 86400000));
}

function esc(s) {
    return String(s ?? "").replace(/[&<>"]/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));
}

// --- Componentes (HTML) ---
function plate(placa) {
    return `
    <div class="plate" title="Placa ${esc(placa)}">
        <div class="plate-bar"><span class="plate-flag"></span>BRASIL</div>
        <div class="plate-chars">${esc(placa)}</div>
    </div>`;
}

function chip(status) {
    const key = String(status || "").toLowerCase();
    const cls = key === "ativa" ? "chip-ativa" : key === "cancelada" ? "chip-cancelada" : "chip-expirada";
    return `<span class="chip ${cls}">${esc(status)}</span>`;
}

function record(a) {
    const ativa = String(a.status || "").toLowerCase() === "ativa";
    const cancelar = ativa
        ? `<button class="link-btn" data-cancel="${a.id}">Cancelar</button>`
        : `<button class="link-btn" disabled>Cancelar</button>`;

    return `
    <article class="record ${ativa ? "" : "is-inativa"}">
        ${plate(a.placa)}
        <div class="record-main">
            <div class="record-num">${esc(a.numeroApolice)}</div>
            <div class="record-holder">Segurado <b>${esc(doc(a.cpfCnpj))}</b></div>
        </div>
        <div class="record-meta">
            <div class="meta">
                <span class="meta-label">Vigência</span>
                <span class="meta-val">${date(a.dataInicioVigencia)} – ${date(a.dataFimVigencia)}</span>
            </div>
            <div class="meta">
                <span class="meta-label">Prêmio</span>
                <span class="meta-val">${money(a.valorPremio)}</span>
            </div>
        </div>
        <div class="record-end">
            <div class="record-status">${chip(a.status)}</div>
            <div class="record-actions">
                ${cancelar}
                <button class="link-btn danger" data-del="${a.id}">Excluir</button>
            </div>
        </div>
    </article>`;
}

// --- Render ---
function renderLista(items) {
    totalCount.textContent = items.length ? `${items.length} no total` : "";
    lista.innerHTML = items.length
        ? items.map(record).join("")
        : `<div class="list-empty">Nenhuma apólice emitida ainda. Use <b>Nova apólice</b> para começar.</div>`;
}

function renderVencendo(items) {
    vencendoCount.textContent = items.length ? `${items.length} apólice(s)` : "";
    vencendoList.innerHTML = items.length
        ? items
              .map(
                  (a) =>
                      `<span class="venc-chip"><b>${esc(a.numeroApolice)}</b><span>vence em ${diasAte(a.dataFimVigencia)} dia(s)</span></span>`
              )
              .join("")
        : `<p class="alert-empty">Nenhuma apólice vence nos próximos 30 dias.</p>`;
}

async function carregar() {
    try {
        const [todas, vencendo] = await Promise.all([
            api("GET", "/apolices"),
            api("GET", "/apolices/vencendo"),
        ]);
        renderLista(todas || []);
        renderVencendo(vencendo || []);
    } catch (err) {
        toast(err.message, true);
    }
}

// --- Ações da lista (delegação) ---
lista.addEventListener("click", async (e) => {
    const cancelar = e.target.closest("[data-cancel]");
    const excluir = e.target.closest("[data-del]");

    if (cancelar) {
        if (!confirm("Cancelar esta apólice? A ação não pode ser desfeita.")) return;
        try {
            await api("POST", `/apolices/${cancelar.dataset.cancel}/cancelar`);
            toast("Apólice cancelada.");
            await carregar();
        } catch (err) {
            toast(err.message, true);
        }
    }

    if (excluir) {
        if (!confirm("Excluir esta apólice permanentemente?")) return;
        try {
            await api("DELETE", `/apolices/${excluir.dataset.del}`);
            toast("Apólice excluída.");
            await carregar();
        } catch (err) {
            toast(err.message, true);
        }
    }
});

// --- Formulário de emissão ---
document.getElementById("btnNova").addEventListener("click", () => {
    form.reset();
    formError.hidden = true;

    const iso = (d) => d.toISOString().substring(0, 10);
    const hoje = new Date();
    const daqui1ano = new Date(hoje);
    daqui1ano.setFullYear(daqui1ano.getFullYear() + 1);
    form.dataInicio.value = iso(hoje);
    form.dataFim.value = iso(daqui1ano);

    dlg.showModal();
});

document.getElementById("btnFechar").addEventListener("click", () => dlg.close());

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    formError.hidden = true;

    const fd = new FormData(form);
    const body = {
        cpfCnpj: String(fd.get("cpfCnpj")).trim(),
        placa: String(fd.get("placa")).trim(),
        valorPremio: Number(fd.get("valorPremio")),
        // Atenção: no POST a API espera "dataIncioVigencia" (typo do backend).
        dataIncioVigencia: fd.get("dataInicio"),
        dataFimVigencia: fd.get("dataFim"),
    };

    try {
        const criada = await api("POST", "/apolices", body);
        dlg.close();
        toast(`Apólice ${criada.numeroApolice} emitida.`);
        await carregar();
    } catch (err) {
        formError.textContent = err.message;
        formError.hidden = false;
    }
});

// --- Toast ---
let toastTimer;
function toast(msg, isError = false) {
    const t = document.getElementById("toast");
    t.textContent = msg;
    t.classList.toggle("is-error", isError);
    t.hidden = false;
    requestAnimationFrame(() => t.setAttribute("data-show", ""));

    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => {
        t.removeAttribute("data-show");
        setTimeout(() => (t.hidden = true), 200);
    }, 3200);
}

// --- Início ---
carregar();
