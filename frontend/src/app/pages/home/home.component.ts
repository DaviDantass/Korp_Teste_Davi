import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  imports: [RouterLink],
  template: `<section class="page"><p class="eyebrow">Sistema de emissão</p><h1>Operação</h1><p class="intro">Acesse produtos e notas fiscais para acompanhar a operação.</p><div class="shortcuts"><a routerLink="/products"><strong>Produtos</strong><span>Consultar e cadastrar produtos</span></a><a routerLink="/invoices"><strong>Notas fiscais</strong><span>Listar e consultar notas</span></a></div></section>`,
  styles: [`.page{max-width:900px;margin:0 auto;padding:72px 32px}.eyebrow{color:#285b34;font-size:11px;letter-spacing:.15em;text-transform:uppercase;font-weight:700}.page h1{font-size:52px;letter-spacing:-.06em;margin:12px 0}.intro{color:#607064}.shortcuts{display:grid;grid-template-columns:repeat(2,1fr);gap:16px;margin-top:44px}.shortcuts a{border:1px solid #d6e2d7;padding:24px;color:#17251b;text-decoration:none}.shortcuts a:hover{border-color:#285b34;background:#f2f8f2}.shortcuts strong,.shortcuts span{display:block}.shortcuts span{font-size:12px;color:#718075;margin-top:8px}@media(max-width:600px){.shortcuts{grid-template-columns:1fr}}`],
})
export class HomeComponent {}
