create extension if not exists "uuid-ossp";

create table coins
(
    id               uuid default uuid_generate_v1() not null,
    blockchain_type  text                            not null,
    transaction_id   text                            not null,
    coin_number      integer                         not null,
    asset_id         text                         not null,
    asset_address    text,
    value            numeric                         not null,
    value_scale      integer                         not null,
    address          text,
    address_tag      text,
    is_deleted       boolean                             not null,
    address_tag_type integer,
    address_nonce    numeric,
    is_spent         boolean                             not null,
    constraint coins_pk
        primary key (id)
);

create unique index coins_blockchain_type_transaction_id_coin_number_uindex
    on coins (blockchain_type, transaction_id, coin_number);
