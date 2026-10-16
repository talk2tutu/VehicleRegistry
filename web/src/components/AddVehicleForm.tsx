import { useState } from 'react'
import type { FormEvent } from 'react'
import { api, ApiError } from '../api/client'
import type { Manufacturer, Vehicle } from '../api/types'

const currentYear = new Date().getFullYear()
const emptyForm = { ownerName: '', manufacturerId: '', yearOfManufacture: '', weightKg: '' }
type FormField = keyof typeof emptyForm

interface Props {
  manufacturers: Manufacturer[]
  onAdded: (vehicle: Vehicle) => void
}

export function AddVehicleForm({ manufacturers, onAdded }: Props) {
  const [form, setForm] = useState(emptyForm)
  const [errors, setErrors] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  function update(field: FormField, value: string) {
    setForm((current) => ({ ...current, [field]: value }))
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setErrors([])
    try {
      const vehicle = await api.createVehicle({
        ownerName: form.ownerName,
        manufacturerId: Number(form.manufacturerId),
        yearOfManufacture: Number(form.yearOfManufacture),
        weightKg: Number(form.weightKg),
      })
      setForm(emptyForm)
      onAdded(vehicle)
    } catch (error) {
      setErrors(
        error instanceof ApiError && error.errors.length > 0
          ? error.errors
          : ['Could not save the vehicle. Please try again.'],
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="card vehicle-form" onSubmit={handleSubmit}>
      <h2>Add a vehicle</h2>

      {errors.length > 0 && (
        <div className="error-box" role="alert">
          <ul>
            {errors.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </div>
      )}

      <div className="form-grid">
        <label>
          Owner's name
          <input
            value={form.ownerName}
            onChange={(e) => update('ownerName', e.target.value)}
            required
            maxLength={200}
          />
        </label>

        <label>
          Manufacturer
          <select
            value={form.manufacturerId}
            onChange={(e) => update('manufacturerId', e.target.value)}
            required
          >
            <option value="">Select…</option>
            {manufacturers.map((m) => (
              <option key={m.id} value={m.id}>
                {m.name}
              </option>
            ))}
          </select>
        </label>

        <label>
          Year of manufacture
          <input
            type="number"
            value={form.yearOfManufacture}
            onChange={(e) => update('yearOfManufacture', e.target.value)}
            required
            min={1886}
            max={currentYear + 1}
            step={1}
          />
        </label>

        <label>
          Weight (kg)
          <input
            type="number"
            value={form.weightKg}
            onChange={(e) => update('weightKg', e.target.value)}
            required
            min={0.01}
            step={0.01}
            inputMode="decimal"
          />
        </label>
      </div>

      <button type="submit" className="primary" disabled={saving}>
        {saving ? 'Saving…' : 'Add vehicle'}
      </button>
    </form>
  )
}