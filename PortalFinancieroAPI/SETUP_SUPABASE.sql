-- Script SQL para Supabase - PAGOS DE SERVICIOS
-- Ejecutar en: Dashboard → SQL Editor → New Query

-- 1. Crear tabla
CREATE TABLE IF NOT EXISTS pagos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    tipo_servicio TEXT NOT NULL,
    monto DECIMAL(10, 2) NOT NULL,
    referencia TEXT NOT NULL,
    estado TEXT DEFAULT 'pendiente',
    fecha_vencimiento TIMESTAMP WITH TIME ZONE,
    fecha_pago TIMESTAMP WITH TIME ZONE,
    observaciones TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 2. Crear índices
CREATE INDEX IF NOT EXISTS idx_pagos_user_id ON pagos(user_id);
CREATE INDEX IF NOT EXISTS idx_pagos_estado ON pagos(estado);
CREATE INDEX IF NOT EXISTS idx_pagos_fecha_vencimiento ON pagos(fecha_vencimiento);

-- 3. Activar RLS
ALTER TABLE pagos ENABLE ROW LEVEL SECURITY;

-- 4. Crear políticas (RLS)
-- Permitir lectura propia
CREATE POLICY "Users can read own pagos"
    ON pagos
    FOR SELECT
    USING (auth.uid() = user_id);

-- Permitir insertar propios
CREATE POLICY "Users can insert own pagos"
    ON pagos
    FOR INSERT
    WITH CHECK (auth.uid() = user_id);

-- Permitir actualizar propios
CREATE POLICY "Users can update own pagos"
    ON pagos
    FOR UPDATE
    USING (auth.uid() = user_id);

-- Permitir eliminar propios pendientes
CREATE POLICY "Users can delete own pending pagos"
    ON pagos
    FOR DELETE
    USING (auth.uid() = user_id AND estado = 'pendiente');
