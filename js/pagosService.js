import { supabase, requireAuth, formatSoles, formatFecha, showToast } from './supabase.js';

// ── Usuario ─────────────────────────
const user = await requireAuth();
document.getElementById('userName').textContent =
  user.user_metadata?.full_name?.split(' ')[0] || user.email;

// ── Logout ─────────────────────────
document.getElementById('btnLogout').addEventListener('click', async () => {
  await supabase.auth.signOut();
  window.location.replace('/index.html');
});

// ── Cargar cuentas ─────────────────────────
const { data: cuentas } = await supabase
  .from('cuentas')
  .select('*')
  .eq('user_id', user.id);

const selectCuenta = document.getElementById('cuentaOrigen');

if (cuentas && cuentas.length > 0) {
  selectCuenta.innerHTML = cuentas.map(c =>
    `<option value="${c.id}">
      ${c.tipo === 'corriente' ? 'Cta. Corriente' : 'Cta. Ahorro'} — ${formatSoles(c.saldo)}
    </option>`
  ).join('');
}

// ── Modal ─────────────────────────
const modal = new bootstrap.Modal(document.getElementById('modalConfirmar'));
let datosPago = null;

// ── Submit formulario ─────────────────────────
document.getElementById('formPago').addEventListener('submit', (e) => {
  e.preventDefault();

  const servicio = document.querySelector('input[name="servicioRadio"]:checked')?.value;
  const contrato = document.getElementById('contrato').value.trim();
  const monto = parseFloat(document.getElementById('montoPago').value);
  const cuentaId = selectCuenta.value;
  const cuentaLabel = selectCuenta.options[selectCuenta.selectedIndex]?.text;

  if (!servicio) return showToast('Selecciona un servicio.', 'warning');
  if (!contrato) return showToast('Ingresa contrato.', 'warning');
  if (!monto || monto <= 0) return showToast('Monto inválido.', 'warning');
  if (!cuentaId) return showToast('Selecciona cuenta.', 'warning');

  const saldoTexto = cuentaLabel.split('S/')[1]?.replace(/,/g, '').trim();
  const saldoActual = parseFloat(saldoTexto);

  if (monto > saldoActual) {
    return showToast('Fondos insuficientes.', 'danger');
  }

  datosPago = { servicio, contrato, monto, cuentaId, cuentaLabel, saldoActual };

  document.getElementById('confServicio').textContent = servicio.toUpperCase();
  document.getElementById('confContrato').textContent = contrato;
  document.getElementById('confMonto').textContent = formatSoles(monto);
  document.getElementById('confCuenta').textContent = cuentaLabel;

  modal.show();
});

// ── Confirmar pago ─────────────────────────
document.getElementById('btnConfirmarPago').addEventListener('click', async () => {
  if (!datosPago) return;

  document.getElementById('btnConfText').classList.add('d-none');
  document.getElementById('btnConfSpinner').classList.remove('d-none');

  try {
    const nuevoSaldo = datosPago.saldoActual - datosPago.monto;

    const { error: errorCuenta } = await supabase
      .from('cuentas')
      .update({ saldo: nuevoSaldo })
      .eq('id', datosPago.cuentaId)
      .eq('user_id', user.id);

    if (errorCuenta) throw errorCuenta;

    const { error: errorPago } = await supabase
      .from('pagos')
      .insert({
        user_id: user.id,
        servicio: datosPago.servicio,
        numero_contrato: datosPago.contrato,
        monto: datosPago.monto,
        estado: 'completado'
      });

    if (errorPago) throw errorPago;

    showToast(`Pago realizado correctamente`, 'success');

    document.getElementById('formPago').reset();
    datosPago = null;

    await cargarHistorial();

    setTimeout(() => location.reload(), 2000);

  } catch (error) {
    console.error(error);
    showToast(`Error: ${error.message}`, 'danger');
  } finally {
    document.getElementById('btnConfText').classList.remove('d-none');
    document.getElementById('btnConfSpinner').classList.add('d-none');
    modal.hide();
  }
});

// ── Historial ─────────────────────────
async function cargarHistorial() {
  const { data: pagos } = await supabase
    .from('pagos')
    .select('*')
    .eq('user_id', user.id)
    .order('fecha', { ascending: false })
    .limit(10);

  const el = document.getElementById('historialPagos');

  const iconos = {
    agua: 'droplet',
    luz: 'lightning',
    cable: 'tv',
    telefono: 'phone',
    gas: 'fire'
  };

  if (!pagos || pagos.length === 0) {
    el.innerHTML = `
      <div class="historial-empty">
        <i class="bi bi-inbox"></i>
        <p>Sin pagos realizados</p>
      </div>
    `;
    return;
  }

  el.innerHTML = `
    ${pagos.map(p => `
      <div class="pago-item">
        <div class="pago-item-header">
          <div class="pago-item-servicio">
            <i class="bi bi-${['agua', 'luz', 'cable', 'telefono', 'gas'].includes(p.servicio) ? 
              {agua: 'droplet-fill', luz: 'lightning-fill', cable: 'tv', telefono: 'telephone-fill', gas: 'fire'}[p.servicio] : 'cart-fill'}"></i>
            <span class="text-capitalize fw-semibold">${p.servicio}</span>
          </div>
          <span class="pago-item-monto">
            ${formatSoles(p.monto)}
          </span>
        </div>
        <div class="pago-item-meta">
          <span class="pago-item-contrato">
            <i class="bi bi-file-text me-1"></i>${p.numero_contrato}
          </span>
          <span class="pago-item-fecha">
            <i class="bi bi-calendar me-1"></i>${formatFecha(p.fecha)}
          </span>
          <span class="pago-item-estado">
            <i class="bi bi-check-circle me-1"></i>Completado
          </span>
        </div>
      </div>
    `).join('')}
  `;
}

cargarHistorial();