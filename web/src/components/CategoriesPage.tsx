import { useEffect, useState } from 'react'
import { api, ApiError } from '../api/client'
import type { Category } from '../api/types'
import { CategoryForm } from './CategoryForm'
import { CategoryIcon } from './CategoryIcon'

const weightFormat = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

function rangeText(category: Category) {
  const from = `${weightFormat.format(category.minWeightKg)} kg`
  return category.maxWeightKg === null
    ? `${from} and above`
    : `${from} to under ${weightFormat.format(category.maxWeightKg)} kg`
}

export function CategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([])
  const [icons, setIcons] = useState<string[]>([])
  const [editing, setEditing] = useState<Category | null>(null)
  const [formVersion, setFormVersion] = useState(0)
  const [reloadKey, setReloadKey] = useState(0)
  const [message, setMessage] = useState<string | null>(null)
  const [errors, setErrors] = useState<string[]>([])

  useEffect(() => {
    let cancelled = false
    Promise.all([api.getCategories(), api.getIcons()])
      .then(([categoryData, iconData]) => {
        if (!cancelled) {
          setCategories(categoryData)
          setIcons(iconData)
        }
      })
      .catch(() => {
        if (!cancelled) setErrors(['Could not load categories. Is the API running?'])
      })
    return () => {
      cancelled = true
    }
  }, [reloadKey])

  function handleSaved(successMessage: string) {
    setMessage(successMessage)
    setErrors([])
    setEditing(null)
    setFormVersion((v) => v + 1)
    setReloadKey((k) => k + 1)
  }

  function startEdit(category: Category) {
    setEditing(category)
    setMessage(null)
    setErrors([])
    setFormVersion((v) => v + 1)
  }

  function cancelEdit() {
    setEditing(null)
    setFormVersion((v) => v + 1)
  }

  async function handleDelete(category: Category) {
    const confirmed = window.confirm(
      `Delete the "${category.name}" category? The category below it will take over its weight range.`,
    )
    if (!confirmed) return

    setMessage(null)
    setErrors([])
    try {
      await api.deleteCategory(category.id)
      handleSaved(`Category "${category.name}" deleted.`)
    } catch (error) {
      setErrors(
        error instanceof ApiError && error.errors.length > 0
          ? error.errors
          : ['Could not delete the category. Please try again.'],
      )
    }
  }

  return (
    <>
      <CategoryForm
        key={`${editing?.id ?? 'new'}-${formVersion}`}
        icons={icons}
        editing={editing}
        onSaved={handleSaved}
        onCancel={cancelEdit}
      />

      <section className="card">
        <h2>Categories</h2>
        <p className="hint">
          Each category starts at its own weight (included) and ends where the next one starts
          (excluded), so there are never gaps or overlaps. The lowest category must start at 0 kg.
        </p>

        {message && (
          <p className="success-box" role="status">
            {message}
          </p>
        )}
        {errors.length > 0 && (
          <div className="error-box" role="alert">
            <ul>
              {errors.map((text) => (
                <li key={text}>{text}</li>
              ))}
            </ul>
          </div>
        )}

        <table className="category-table">
          <thead>
            <tr>
              <th>Icon</th>
              <th>Name</th>
              <th>Weight range</th>
              <th className="actions-header">
                <span className="visually-hidden">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {categories.map((category) => {
              const isLowest = category.minWeightKg === 0
              return (
                <tr key={category.id}>
                  <td>
                    <CategoryIcon iconKey={category.iconKey} label={category.name} />
                  </td>
                  <td>{category.name}</td>
                  <td>{rangeText(category)}</td>
                  <td>
                    <div className="row-actions">
                      <button type="button" className="small-button" onClick={() => startEdit(category)}>
                        Edit
                      </button>
                      <button
                        type="button"
                        className="small-button danger"
                        onClick={() => handleDelete(category)}
                        disabled={isLowest}
                        title={isLowest ? 'The lowest category cannot be deleted: every weight needs a category.' : undefined}
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </section>
    </>
  )
}