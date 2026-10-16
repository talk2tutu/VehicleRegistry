import type { SortDirection, Vehicle, VehicleSortField } from '../api/types'
import { CategoryIcon } from './CategoryIcon'

const columns: { field: VehicleSortField; label: string; numeric?: boolean }[] = [
  { field: 'owner', label: "Owner's name" },
  { field: 'manufacturer', label: 'Manufacturer' },
  { field: 'year', label: 'Year', numeric: true },
  { field: 'weight', label: 'Weight (kg)', numeric: true },
]

interface Props {
  vehicles: Vehicle[]
  sortBy: VehicleSortField
  sortDir: SortDirection
  loading: boolean
  onSort: (field: VehicleSortField) => void
}

const weightFormat = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

export function VehicleList({ vehicles, sortBy, sortDir, loading, onSort }: Props) {
  if (!loading && vehicles.length === 0) {
    return <p className="empty">No vehicles yet. Add the first one above.</p>
  }

  return (
    <table className="vehicle-table" aria-busy={loading}>
      <thead>
        <tr>
          {columns.map((column) => {
            const active = column.field === sortBy
            const direction = sortDir === 'asc' ? 'ascending' : 'descending'
            return (
              <th
                key={column.field}
                className={column.numeric ? 'numeric' : undefined}
                aria-sort={active ? direction : 'none'}
              >
                <button
                  type="button"
                  className={active ? 'sort-button active' : 'sort-button'}
                  onClick={() => onSort(column.field)}
                >
                  {column.label}
                  <span aria-hidden="true" className="sort-arrow">
                    {active ? (sortDir === 'asc' ? '▲' : '▼') : '↕'}
                  </span>
                </button>
              </th>
            )
          })}
          <th>Category</th>
        </tr>
      </thead>
      <tbody>
        {vehicles.map((vehicle) => (
          <tr key={vehicle.id}>
            <td>{vehicle.ownerName}</td>
            <td>{vehicle.manufacturerName}</td>
            <td className="numeric">{vehicle.yearOfManufacture}</td>
            <td className="numeric">{weightFormat.format(vehicle.weightKg)}</td>
            <td>
              <CategoryIcon iconKey={vehicle.categoryIconKey} label={vehicle.categoryName} />{' '}
              {vehicle.categoryName}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}