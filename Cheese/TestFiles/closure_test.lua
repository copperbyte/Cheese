function closure_test() 
  local x = 1;
  local cc = function(p) 
    x = p + 2;
    return x;
  end
  
  print(x);
  local y = cc(x);
  print(x, y);
  local z = cc(x);
  print(x, y, z);
end

------------

function close_test() 
  local x = 1;
  print(x);

  do
    local y = 3;
    ccc = function(p)
      x = x + p; 
      y = y + p;
      print(x,y,p);
    end
    print(y);
  end
  
  ccc(5);  
  print(x,y); 
  ccc(2);
  print(x,y); 

end

--------

closure_test();

close_test();

