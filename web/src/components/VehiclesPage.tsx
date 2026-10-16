import { useEffect, useState } from 'react'
import { api } from '../api/client'
import type { Manufacturer, SortDirection, Vehicle, VehicleSortField } from '../api/types'
import { AddVehicleForm } from './AddVehicleForm'
import { VehicleList } from './VehicleList'

export function VehiclesPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([])
  const [manufacturers, setManufacturers] = useState<Manufacturer[]>([])
  const [sortBy, setSortBy] = useState<VehicleSortField>('owner')
  const [sortDir, setSortDir] = useState<SortDirection>('asc')
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    api.getManufacturers()
      .then(setManufacturers)
      .catch(() => setLoadError('Could not load manufacturers. Is the API running?'))
  }, [])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    api.getVehicles(sortBy, sortDir)
      .then((data) => {
        if (!cancelled) {
          setVehicles(data)
          setLoadError(null)
        }
      })
      .catch(() => {
        if (!cancelled) setLoadError('Could not load vehicles. Is the API running?')
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [sortBy, sortDir, reloadKey])

  function handleSort(field: VehicleSortField) {
    if (field === sortBy) {
      setSortDir((current) => (current === 'asc' ? 'desc' : 'asc'))
    } else {
      setSortBy(field)
      setSortDir('asc')
    }
  }

  return (
    <>
      <AddVehicleForm manufacturers={manufacturers} onAdded={() => setReloadKey((k) => k + 1)} />

      <section className="card">
        <h2>Vehicles</h2>
        {loadError && (
          <p className="error-box" role="alert">
            {loadError}
          </p>
        )}
        <VehicleList
          vehicles={vehicles}
          sortBy={sortBy}
          sortDir={sortDir}
          loading={loading}
          onSort={handleSort}
        />
      </section>
    </>
  )
}