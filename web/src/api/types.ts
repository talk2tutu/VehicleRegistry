export interface Manufacturer {
  id: number
  name: string
}

export interface Vehicle {
  id: number
  ownerName: string
  manufacturerId: number
  manufacturerName: string
  yearOfManufacture: number
  weightKg: number
  categoryId: number
  categoryName: string
  categoryIconKey: string
}

export interface CreateVehicleRequest {
  ownerName: string
  manufacturerId: number
  yearOfManufacture: number
  weightKg: number
}

export interface Category {
  id: number
  name: string
  minWeightKg: number
  maxWeightKg: number | null
  iconKey: string
}

export interface CategoryRequest {
  name: string
  minWeightKg: number
  iconKey: string
}

export type VehicleSortField = 'owner' | 'manufacturer' | 'year' | 'weight'
export type SortDirection = 'asc' | 'desc'