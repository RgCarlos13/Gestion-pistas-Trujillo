const TrujilloVialMapa = (() => {
    // Área piloto: Centro Histórico de Trujillo delimitado por el anillo de la Av. España.
    // El polígono se ajusta al contorno del centro histórico, no a un círculo genérico.
    const centroTrujillo = [-8.11065, -79.02775];
    const zoomInicial = 16;
    const poligonoCentroHistorico = [
        [-8.10600, -79.03415],
        [-8.10445, -79.03265],
        [-8.10395, -79.03055],
        [-8.10420, -79.02825],
        [-8.10505, -79.02580],
        [-8.10645, -79.02360],
        [-8.10850, -79.02165],
        [-8.11075, -79.02085],
        [-8.11315, -79.02145],
        [-8.11505, -79.02335],
        [-8.11625, -79.02585],
        [-8.11665, -79.02865],
        [-8.11605, -79.03120],
        [-8.11445, -79.03320],
        [-8.11225, -79.03435],
        [-8.10985, -79.03475],
        [-8.10765, -79.03455]
    ];
    const distanciaMaximaAPistaMetros = 25;
    const mapaRefs = {};

    function colorPorEstado(estado) {
        const normalizado = String(estado || '').toLowerCase();
        if (normalizado === 'bueno' || normalizado === '0') return '#198754';
        if (normalizado === 'regular' || normalizado === '1') return '#ffc107';
        if (normalizado === 'malo' || normalizado === '2') return '#fd7e14';
        if (normalizado === 'enreparacion' || normalizado === '3') return '#0d6efd';
        if (normalizado === 'intransitable' || normalizado === '4') return '#dc3545';
        return '#0d6efd';
    }

    function textoEstadoDesdeSelect(valor) {
        const mapa = {
            '0': 'Bueno',
            '1': 'Regular',
            '2': 'Malo',
            '3': 'EnReparacion',
            '4': 'Intransitable'
        };
        return mapa[valor] || valor || 'Bueno';
    }

    function crearMapa(elementId) {
        const contenedor = document.getElementById(elementId);
        if (!contenedor || typeof L === 'undefined') return null;

        const mapa = L.map(elementId).setView(centroTrujillo, zoomInicial);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap'
        }).addTo(mapa);

        mostrarZonaPermitida(mapa);
        setTimeout(() => mapa.invalidateSize(), 250);
        return mapa;
    }

    function mostrarZonaPermitida(mapa) {
        const zona = L.polygon(poligonoCentroHistorico, {
            color: '#0d6efd',
            weight: 2,
            opacity: 0.85,
            fillColor: '#0d6efd',
            fillOpacity: 0.07,
            dashArray: '8, 8'
        }).addTo(mapa);

        zona.bindTooltip('Área permitida: Centro Histórico de Trujillo', {
            permanent: false,
            direction: 'top'
        });

        mapa.fitBounds(zona.getBounds(), { padding: [18, 18] });
        mapa.setMaxBounds(zona.getBounds().pad(0.25));
    }

    function puntoDentroCentro(mapa, punto) {
        let dentro = false;
        const lat = Number(punto.lat);
        const lng = Number(punto.lng);

        for (let i = 0, j = poligonoCentroHistorico.length - 1; i < poligonoCentroHistorico.length; j = i++) {
            const latI = poligonoCentroHistorico[i][0];
            const lngI = poligonoCentroHistorico[i][1];
            const latJ = poligonoCentroHistorico[j][0];
            const lngJ = poligonoCentroHistorico[j][1];

            const cruza = ((latI > lat) !== (latJ > lat)) &&
                (lng < (lngJ - lngI) * (lat - latI) / (latJ - latI) + lngI);
            if (cruza) dentro = !dentro;
        }

        return dentro;
    }

    function escapeHtml(value) {
        return String(value ?? '')
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');
    }

    function parseRuta(tramo) {
        if (tramo?.rutaCoordenadas) {
            try {
                const parsed = JSON.parse(tramo.rutaCoordenadas);
                if (Array.isArray(parsed) && parsed.length >= 2) return parsed;
            } catch { }
        }

        if (tramo?.latInicio && tramo?.lngInicio && tramo?.latFin && tramo?.lngFin) {
            return [
                [Number(tramo.latInicio), Number(tramo.lngInicio)],
                [Number(tramo.latFin), Number(tramo.lngFin)]
            ];
        }

        return [];
    }

    function pintarTramo(mapa, tramo, opciones = {}) {
        const ruta = parseRuta(tramo);
        if (!mapa || ruta.length < 2) return null;

        const color = colorPorEstado(tramo.estado);
        const linea = L.polyline(ruta, {
            color,
            weight: opciones.weight || 6,
            opacity: 0.95
        }).addTo(mapa);

        const popup = `
            <strong>Tramo #${escapeHtml(tramo.id)}</strong><br>
            <span>${escapeHtml(tramo.direccionReferencia)}</span><br>
            <span>Estado: <strong>${escapeHtml(tramo.estado)}</strong></span><br>
            <a href="/Tramos/Detalle/${encodeURIComponent(tramo.id)}">Ver detalle</a>
        `;
        linea.bindPopup(popup);

        if (opciones.marcadores !== false) {
            L.circleMarker(ruta[0], { radius: 6, color, fillColor: color, fillOpacity: 1 }).addTo(mapa).bindTooltip('Inicio');
            L.circleMarker(ruta[ruta.length - 1], { radius: 6, color, fillColor: color, fillOpacity: 1 }).addTo(mapa).bindTooltip('Fin');
        }

        return linea;
    }

    async function ajustarAPista(punto, mapa) {
        const url = `https://router.project-osrm.org/nearest/v1/driving/${punto.lng},${punto.lat}?number=1`;
        try {
            const resp = await fetch(url);
            if (!resp.ok) throw new Error('OSRM nearest no disponible');

            const data = await resp.json();
            const waypoint = data?.waypoints?.[0];
            const location = waypoint?.location;
            const distancia = Number(waypoint?.distance ?? 9999);

            if (!Array.isArray(location) || location.length < 2) {
                throw new Error('No se encontró una vía cercana');
            }

            if (distancia > distanciaMaximaAPistaMetros) {
                return {
                    ok: false,
                    mensaje: 'Selecciona un punto sobre una pista o calle del centro histórico.'
                };
            }

            const ajustado = { lat: Number(location[1]), lng: Number(location[0]), distancia };
            if (!puntoDentroCentro(mapa, ajustado)) {
                return {
                    ok: false,
                    mensaje: 'La pista más cercana queda fuera del centro histórico permitido.'
                };
            }

            return { ok: true, punto: ajustado };
        } catch {
            return {
                ok: false,
                mensaje: 'No se pudo validar la pista. Revisa tu conexión e intenta nuevamente.'
            };
        }
    }

    async function obtenerRutaOSRM(inicio, fin, mapa) {
        const url = `https://router.project-osrm.org/route/v1/driving/${inicio.lng},${inicio.lat};${fin.lng},${fin.lat}?overview=full&geometries=geojson`;
        try {
            const resp = await fetch(url);
            if (!resp.ok) throw new Error('OSRM no disponible');
            const data = await resp.json();
            const coords = data?.routes?.[0]?.geometry?.coordinates;
            if (!Array.isArray(coords) || coords.length < 2) throw new Error('Ruta OSRM vacía');

            const ruta = coords.map(c => [c[1], c[0]]);
            let rutaDentroDelCentro = true;
            for (let i = 0; i < ruta.length; i++) {
                const puntoRuta = { lat: ruta[i][0], lng: ruta[i][1] };
                if (!puntoDentroCentro(mapa, puntoRuta)) {
                    rutaDentroDelCentro = false;
                    break;
                }
            }

            if (!rutaDentroDelCentro) {
                return {
                    ok: false,
                    mensaje: 'La ruta calculada sale del centro histórico permitido. Selecciona otro tramo.'
                };
            }

            return { ok: true, ruta };
        } catch {
            return {
                ok: false,
                mensaje: 'No se pudo calcular una ruta por pista. Intenta con puntos más cercanos sobre la calle.'
            };
        }
    }

    function initCrear(elementId, opciones) {
        const mapa = crearMapa(elementId);
        if (!mapa) return;

        let inicio = null;
        let fin = null;
        let linea = null;
        let marcadorInicio = null;
        let marcadorFin = null;

        const estadoSelect = document.getElementById(opciones.estadoSelectId);
        const ayuda = document.getElementById(opciones.ayudaId);
        const paso = document.getElementById(opciones.pasoId);
        const btnLimpiar = document.getElementById(opciones.btnLimpiarId);
        const inputs = {
            latInicio: document.getElementById(opciones.inputs.latInicio),
            lngInicio: document.getElementById(opciones.inputs.lngInicio),
            latFin: document.getElementById(opciones.inputs.latFin),
            lngFin: document.getElementById(opciones.inputs.lngFin),
            ruta: document.getElementById(opciones.inputs.ruta)
        };

        function estadoActual() {
            return textoEstadoDesdeSelect(estadoSelect?.value || '0');
        }

        function actualizarAyuda(texto, pasoTexto) {
            if (ayuda) ayuda.textContent = texto;
            if (paso) paso.textContent = pasoTexto;
        }

        function limpiar() {
            inicio = null;
            fin = null;
            if (linea) mapa.removeLayer(linea);
            if (marcadorInicio) mapa.removeLayer(marcadorInicio);
            if (marcadorFin) mapa.removeLayer(marcadorFin);
            linea = null;
            marcadorInicio = null;
            marcadorFin = null;
            if (inputs.latInicio) inputs.latInicio.value = '';
            if (inputs.lngInicio) inputs.lngInicio.value = '';
            if (inputs.latFin) inputs.latFin.value = '';
            if (inputs.lngFin) inputs.lngFin.value = '';
            if (inputs.ruta) inputs.ruta.value = '';
            actualizarAyuda('Selecciona el punto de inicio sobre una pista del centro histórico.', 'Paso 1');
        }

        async function dibujarRuta() {
            if (!inicio || !fin) return;
            if (linea) mapa.removeLayer(linea);

            actualizarAyuda('Calculando ruta por pista...', 'Validando');
            const resultadoRuta = await obtenerRutaOSRM(inicio, fin, mapa);
            if (!resultadoRuta.ok) {
                if (marcadorFin) mapa.removeLayer(marcadorFin);
                fin = null;
                marcadorFin = null;
                if (inputs.latFin) inputs.latFin.value = '';
                if (inputs.lngFin) inputs.lngFin.value = '';
                if (inputs.ruta) inputs.ruta.value = '';
                actualizarAyuda(resultadoRuta.mensaje, 'Ruta inválida');
                return;
            }

            const ruta = resultadoRuta.ruta;
            const color = colorPorEstado(estadoActual());
            linea = L.polyline(ruta, { color, weight: 7, opacity: 0.95 }).addTo(mapa);
            mapa.fitBounds(linea.getBounds(), { padding: [30, 30] });

            if (inputs.latInicio) inputs.latInicio.value = inicio.lat.toFixed(7);
            if (inputs.lngInicio) inputs.lngInicio.value = inicio.lng.toFixed(7);
            if (inputs.latFin) inputs.latFin.value = fin.lat.toFixed(7);
            if (inputs.lngFin) inputs.lngFin.value = fin.lng.toFixed(7);
            if (inputs.ruta) inputs.ruta.value = JSON.stringify(ruta);
            actualizarAyuda('Tramo seleccionado sobre pista. Puedes registrar el reporte o limpiar el mapa.', 'Listo');
        }

        mapa.on('click', async (e) => {
            const puntoClic = { lat: e.latlng.lat, lng: e.latlng.lng };
            if (!puntoDentroCentro(mapa, puntoClic)) {
                actualizarAyuda('Solo se pueden registrar tramos dentro del centro histórico de Trujillo.', 'Fuera de zona');
                return;
            }

            actualizarAyuda('Validando que el punto esté sobre una pista...', 'Validando');
            const resultadoAjuste = await ajustarAPista(puntoClic, mapa);
            if (!resultadoAjuste.ok) {
                actualizarAyuda(resultadoAjuste.mensaje, 'Punto inválido');
                return;
            }

            const punto = resultadoAjuste.punto;

            if (!inicio || (inicio && fin)) {
                limpiar();
                inicio = punto;
                marcadorInicio = L.marker([punto.lat, punto.lng]).addTo(mapa).bindPopup('Inicio ajustado a la pista más cercana').openPopup();
                actualizarAyuda('Punto inicial válido. Ahora selecciona el punto final sobre una pista.', 'Paso 2');
                return;
            }

            fin = punto;
            marcadorFin = L.marker([punto.lat, punto.lng]).addTo(mapa).bindPopup('Fin ajustado a la pista más cercana');
            if (marcadorInicio) marcadorInicio.setIcon(new L.Icon.Default());
            await dibujarRuta();
        });

        estadoSelect?.addEventListener('change', () => {
            if (linea) linea.setStyle({ color: colorPorEstado(estadoActual()) });
        });

        btnLimpiar?.addEventListener('click', limpiar);

        // Restaurar selección si el formulario volvió con errores de validación.
        const latI = Number(inputs.latInicio?.value || 0);
        const lngI = Number(inputs.lngInicio?.value || 0);
        const latF = Number(inputs.latFin?.value || 0);
        const lngF = Number(inputs.lngFin?.value || 0);
        if (latI && lngI && latF && lngF) {
            inicio = { lat: latI, lng: lngI };
            fin = { lat: latF, lng: lngF };
            marcadorInicio = L.marker([latI, lngI]).addTo(mapa).bindPopup('Inicio del tramo');
            marcadorFin = L.marker([latF, lngF]).addTo(mapa).bindPopup('Fin del tramo');
            dibujarRuta();
        }
    }

    function initDetalle(elementId, tramo) {
        const mapa = crearMapa(elementId);
        if (!mapa) return;

        const linea = pintarTramo(mapa, tramo, { weight: 8 });
        if (linea) mapa.fitBounds(linea.getBounds(), { padding: [40, 40] });
    }

    function initIndex(elementId, tramos) {
        const mapa = crearMapa(elementId);
        if (!mapa) return;

        const lineas = {};
        const bounds = [];
        (tramos || []).forEach(tramo => {
            const linea = pintarTramo(mapa, tramo);
            if (linea) {
                lineas[tramo.id] = linea;
                linea.getLatLngs().forEach(p => bounds.push(p));
            }
        });

        mapaRefs.general = { mapa, lineas };
        if (bounds.length > 0) mapa.fitBounds(bounds, { padding: [30, 30] });
    }

    function enfocarTramo(id) {
        const ref = mapaRefs.general;
        if (!ref || !ref.lineas[id]) return;
        const linea = ref.lineas[id];
        ref.mapa.fitBounds(linea.getBounds(), { padding: [40, 40], maxZoom: 18 });
        linea.openPopup();
        document.getElementById('mapaGeneral')?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }

    return {
        initCrear,
        initDetalle,
        initIndex,
        enfocarTramo
    };
})();
