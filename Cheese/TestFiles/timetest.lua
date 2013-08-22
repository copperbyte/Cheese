accum = 0
bins = { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }

function func(x)
--func = function(x) do
  local index = (x % 16) + 1
  bins[index] = bins[index] + x
  accum = accum + x
end



for loop=1,1000000,1 do
    func(loop)
end


print(accum)
for k,v in ipairs(bins) do 
  print(k,v) 
end
