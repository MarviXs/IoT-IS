export type ARESQueryParams = {
  ico: string | undefined;
  obchodniJmeno: string | undefined;
  start: number;
  pocet: number;
};

export type ARESResponse = {
  pocetCelkem: 1;
  ekonomickeSubjekty: [EkonomickySubjekt];
};

export type EkonomickySubjekt = {
  ico: string;
  obchodniJmeno: string;
  sidlo: {
    nazevObce: string;
    nazevUlice: string;
    cisloDomovni: number;
    cisloOrientacni: number;
    psc: string;
    textovaAdresa: string;
  };
  dic: string;
};

class ARESService {
  async getCompanies(queryParams: ARESQueryParams) {
    const query = {
      ico: queryParams.ico,
      obchodniJmeno: queryParams.obchodniJmeno,
      start: queryParams.start,
      pocet: queryParams.pocet,
    };

    const response = await fetch(`https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/ekonomicke-subjekty/vyhledat`, {
      method: 'POST',
      body: JSON.stringify(query),
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Network response was not ok');
    }

    const data: ARESResponse = await response.json();
    return data;
  }
}

export default new ARESService();
