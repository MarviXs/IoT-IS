import { client } from '@/api/client';
import type { paths } from '@/api/generated/schema.d.ts';

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

export default new CompanyService();
