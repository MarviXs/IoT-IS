import { client } from '@/api/client';
//import type { paths } from '@/api/generated/schema.d.ts';

export type Company = {
    id: number;
    name: string;
    name2: string | null | undefined;
    ic: string;
    dic: string | null | undefined;
    street: string | null | undefined;
    psc: string | null | undefined;
    city: string | null | undefined;
  };
  
  // Define CreateCompanyParams for creating a new company (excluding ID)
  export type CreateCompanyParams = Omit<Company, 'id'>;
  
  class CompanyService {
    // Mocked data
    private companies: Company[] = [
      {
        id: 1,
        name: 'Company A',
        name2: 'Secondary A',
        ic: '123456',
        dic: '654321',
        street: 'Main St',
        psc: '11111',
        city: 'City A',
      },
      {
        id: 2,
        name: 'Company B',
        name2: 'Secondary B',
        ic: '789012',
        dic: '210987',
        street: '2nd St',
        psc: '22222',
        city: 'City B',
      },
    ];
  
    async getCompanies(): Promise<{ data: Company[]; error?: any }> {
        return new Promise<{ data: Company[]; error?: any }>((resolve, reject) => {
          setTimeout(() => {
            // Simulating an error for testing purposes
            const isError = false; // Change this to simulate an error
            if (isError) {
              reject({
                error: { message: 'Failed to fetch companies', status: 500 }
              });
            } else {
              resolve({ data: this.companies });
            }
          }, 300);
        });
      }
  
    // Get a specific company by ID
    async getCompany(companyId: number): Promise<Company | undefined> {
      return new Promise<Company | undefined>((resolve) => {
        const company = this.companies.find((c) => c.id === companyId);
        setTimeout(() => resolve(company), 300);
      });
    }
  
    // Create a new company
    async createCompany(company: CreateCompanyParams): Promise<{ data: Company; error?: { errors: { message: string }; status?: number } }> {
      return new Promise<{ data: Company; error?: { errors: { message: string }; status?: number } }>((resolve, reject) => {
        try {
          const newCompany = { ...company, id: Date.now() }; // Mock ID generation
          this.companies.push(newCompany);
          setTimeout(() => resolve({ data: newCompany }), 300);
        } catch (error) {
          // Handle any unexpected error
          setTimeout(() => reject({
            error: {
              errors: { message: 'Failed to create company' },
              status: 500,
            }
          }), 300);
        }
      });
    }
  
    // Update an existing company
    async updateCompany(companyId: number, updatedData: Partial<Company>): Promise<Company | undefined> {
      return new Promise<Company | undefined>((resolve) => {
        const company = this.companies.find((c) => c.id === companyId);
        if (company) {
          Object.assign(company, updatedData);
        }
        setTimeout(() => resolve(company), 300);
      });
    }
  
    // Delete a company
    async deleteCompany(companyId: number): Promise<boolean> {
      return new Promise<boolean>((resolve) => {
        const index = this.companies.findIndex((c) => c.id === companyId);
        if (index !== -1) {
          this.companies.splice(index, 1);
          resolve(true);
        } else {
          resolve(false);
        }
      });
    }
  }
  
  // Export an instance of the service for use in other parts of the app
  export default new CompanyService();


/*
export type CompaniesQueryParams = paths['/companies']['get']['parameters']['query'];
export type CompaniesResponse = paths['/companies']['get']['responses']['200']['content']['application/json'];

export type CompanyResponse = paths['/companies/{id}']['get']['responses']['200']['content']['application/json'];

export type CreateCompanyParams = paths['/companies']['post']['requestBody']['content']['application/json'];
export type CreateCompanyResponse = paths['/companies']['post']['responses']['201']['content']['application/json'];

export type UpdateCompanyRequest = paths['/companies/{id}']['put']['requestBody']['content']['application/json'];

class CompanyService {
  async getCompanies(queryParams: CompaniesQueryParams) {
    return await client.GET('/companies', { params: { query: queryParams } });
  }

  async getCompany(companyId: string) {
    return await client.GET('/companies/{id}', { params: { path: { id: companyId } } });
  }

  async createCompany(body: CreateCompanyParams) {
    return await client.POST('/companies', { body });
  }

  async updateCompany(companyId: string, body: UpdateCompanyRequest) {
    return await client.PUT('/companies/{id}', { body, params: { path: { id: companyId } } });
  }

  async deleteCompany(companyId: string) {
    return await client.DELETE('/companies/{id}', { params: { path: { id: companyId } } });
  }
}

export default new CompanyService();*/
