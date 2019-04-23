create extension if not exists "uuid-ossp";

BEGIN;
create or replace function set_numeric_value_from_string () returns trigger AS '
begin
     NEW.value= cast(NEW.value_string as numeric);
     return NEW;
end;
 ' language plpgsql;

create table coins
(
    id               uuid default uuid_generate_v1() not null,
    blockchain_type  text                            not null,
    transaction_id   text                            not null,
    coin_number      integer                         not null,
    asset_id         text                            not null,
    coin_id          text                            not null,
    asset_address    text,
    value            numeric                         not null,
    value_string     text                            not null,
    value_scale      integer                         not null,
    address          text,
    address_tag      text,
    is_deleted       boolean                         not null,
    address_tag_type integer,
    address_nonce    numeric,
    is_spent         boolean                         not null,
    constraint coins_pk
        primary key (id)
);

create unique index coins_natural_key_index
    on coins (blockchain_type, coin_id);

create index coins_blockchain_type_transaction_id_index
	on coins (blockchain_type, transaction_id);

create  trigger set_numeric_value_from_string_trigger before insert or update
     on coins for each row
     execute procedure set_numeric_value_from_string ();

create table fees
(
    id              uuid default uuid_generate_v1() not null,
    blockchain_type text                            not null,
    block_id        text                            not null,
    transaction_id  text                            not null,
    asset_id        text                            not null,
    asset_address   text,
    value           numeric                         not null,
    value_string    text                            not null,
    value_scale     integer                         not null,
    constraint fees_pk
        primary key (id)
);

create unique index fees_natural_key_index_1
    on fees (blockchain_type, transaction_id, asset_id)
where asset_address is null;

create unique index fees_natural_key_index_2
    on fees (blockchain_type, transaction_id, asset_id, asset_address)
where asset_address is not null;

create index fees_blockchain_type_block_id_index
	on fees (blockchain_type, block_id);


create index fees_blockchain_type_transaction_id_index
	on fees (blockchain_type, transaction_id);


create  trigger set_numeric_value_from_string_trigger before insert or update
     on fees for each row
     execute procedure set_numeric_value_from_string ();

create table balance_actions
(
    id              uuid default uuid_generate_v1(),
    blockchain_type text    not null,
    block_id        text    not null,
    block_number    integer not null,
    asset_id        text    not null,
    asset_address   text,
    transaction_id  text    not null,
    value           numeric not null,
    value_string    text    not null,
    value_scale     integer not null,
    address         text    not null
);

create unique index balance_actions_natural_key_index_1
    on balance_actions (blockchain_type, transaction_id, address, asset_id)
    where asset_address is null;

create unique index balance_actions_natural_key_index_2
    on balance_actions (blockchain_type, transaction_id, address, asset_id, asset_address)
    where asset_address is not null;

create index query_covered_by_address
    on balance_actions (blockchain_type, address, block_number desc, asset_id, asset_address, value);

create  trigger set_numeric_value_from_string_trigger before insert or update
     on balance_actions for each row
     execute procedure set_numeric_value_from_string ();



create table assets
(
    id              text                            not null,
    asset_id        text                            not null,
    asset_address   text,
	blockchain_type text                            not null,
	scale int not null,
	constraint assets_pk
		primary key (blockchain_type, id)
);

COMMIT;