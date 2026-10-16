import { useState } from 'react'
import type { FormEvent } from 'react'
import { api, ApiError } from '../api/client'
import type { Category } from '../api/types'
import { CategoryIcon } from './CategoryIcon'

interface Props {
  icons: string[]
  editing: Category | null
  onSaved: (message: string) => void
  onCancel: () => void
}

export function CategoryForm({ icons, editing, onSaved, onCancel }: Props) {
  const [name, setName] = useState(editing?.name ?? '')
  const [minWeightKg, setMinWeightKg] = useState(editing ? String(editing.minWeightKg) : '')
  const [iconKey, setIconKey] = useState(editing?.iconKey ?? '')
  const [errors, setErrors] = useState<string[]>([])
  const [saving, setSaving] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setErrors([])
    const body = { name, minWeightKg: Number(minWeightKg), iconKey }
    try {
      if (editing) {
        await api.updateCategory(editing.id, body)
        onSaved(`Category "${name}" updated. Vehicle categories update straight away.`)
      } else {
        await api.createCategory(body)
        onSaved(`Category "${name}" added.`)
      }
    } catch (error) {
      setErrors(
        error instanceof ApiError && error.errors.length > 0
          ? error.errors
          : ['Could not save the category. Please try again.'],
      )
    } finally {
      setSaving(false)
    }
  }

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>{editing ? `Edit "${editing.name}"` : 'Add a category'}</h2>

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
          Name
          <input value={name} onChange={(e) => setName(e.target.value)} required maxLength={50} />
        </label>

        <label>
          Starts at (kg)
          <input
            type="number"
            value={minWeightKg}
            onChange={(e) => setMinWeightKg(e.target.value)}
            required
            min={0}
            step={0.01}
            inputMode="decimal"
          />
        </label>
      </div>

      <fieldset className="icon-picker">
        <legend>Icon</legend>
        <div className="icon-options">
          {icons.map((key) => (
            <label key={key} className={iconKey === key ? 'icon-option selected' : 'icon-option'}>
              <input
                type="radio"
                name="iconKey"
                value={key}
                checked={iconKey === key}
                onChange={() => setIconKey(key)}
                required
              />
              <CategoryIcon iconKey={key} label={key} />
              <span>{key}</span>
            </label>
          ))}
        </div>
      </fieldset>

      <div className="form-actions">
        <button type="submit" className="primary" disabled={saving}>
          {saving ? 'Saving…' : editing ? 'Save changes' : 'Add category'}
        </button>
        {editing && (
          <button type="button" className="secondary" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  )
}