tbl = { ['a']=1, ['b']=2 }
tbl['b'] = (tbl['b'] or 0) + 2;
tbl['c'] = (tbl['c'] or 0) + 3;
print(tbl['b'],tbl['c']);
